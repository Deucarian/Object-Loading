using System;
using System.Collections.Generic;
using Deucarian.Diagnostics;

namespace Deucarian.ObjectLoading.Diagnostics
{
    public static class ObjectLoadingDiagnostics
    {
        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<IObjectLoadingDiagnosticsSource, RegistrationState> Registrations =
            new Dictionary<IObjectLoadingDiagnosticsSource, RegistrationState>();

        public static IDisposable Register(IObjectLoadingDiagnosticsSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            lock (SyncRoot)
            {
                RegistrationState state;
                if (Registrations.TryGetValue(source, out state) && IsProviderRegistered(state.Provider))
                {
                    return state.AddReference();
                }

                if (state != null)
                {
                    state.DisposeRegistration();
                    Registrations.Remove(source);
                }

                ObjectLoadingDiagnosticProvider provider = new ObjectLoadingDiagnosticProvider(source);
                DiagnosticProviderRegistration registration = DiagnosticProviderRegistry.Register(provider);
                state = new RegistrationState(source, provider, registration);
                Registrations[source] = state;
                return state.AddReference();
            }
        }

        private static bool IsProviderRegistered(ObjectLoadingDiagnosticProvider provider)
        {
            IReadOnlyList<IDiagnosticProvider> providers = DiagnosticProviderRegistry.SnapshotProviders();
            for (int i = 0; i < providers.Count; i++)
            {
                if (object.ReferenceEquals(providers[i], provider))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Release(RegistrationState state)
        {
            if (state == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (!state.ReleaseReference())
                {
                    return;
                }

                RegistrationState registeredState;
                if (Registrations.TryGetValue(state.Source, out registeredState)
                    && object.ReferenceEquals(registeredState, state))
                {
                    Registrations.Remove(state.Source);
                }

                state.DisposeRegistration();
            }
        }

        private sealed class RegistrationState
        {
            private readonly DiagnosticProviderRegistration registration;
            private int referenceCount;
            private bool disposed;

            public RegistrationState(IObjectLoadingDiagnosticsSource source,
                                     ObjectLoadingDiagnosticProvider provider,
                                     DiagnosticProviderRegistration registration)
            {
                Source = source;
                Provider = provider;
                this.registration = registration;
            }

            public IObjectLoadingDiagnosticsSource Source { get; }
            public ObjectLoadingDiagnosticProvider Provider { get; }

            public IDisposable AddReference()
            {
                referenceCount++;
                return new RegistrationLease(this);
            }

            public bool ReleaseReference()
            {
                if (referenceCount > 0)
                {
                    referenceCount--;
                }

                return referenceCount == 0;
            }

            public void DisposeRegistration()
            {
                if (disposed)
                {
                    return;
                }

                registration.Dispose();
                disposed = true;
            }
        }

        private sealed class RegistrationLease : IDisposable
        {
            private RegistrationState state;

            public RegistrationLease(RegistrationState state)
            {
                this.state = state;
            }

            public void Dispose()
            {
                RegistrationState current = state;
                if (current == null)
                {
                    return;
                }

                state = null;
                Release(current);
            }
        }
    }
}
