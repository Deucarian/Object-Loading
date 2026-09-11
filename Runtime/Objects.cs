using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deucarian.ObjectLoading
{
    /// <summary>Convenience access to the explicitly configured object loading host.</summary>
    public static class Objects
    {
        private static Registration current;
        public static bool IsConfigured => current != null && current.Host != null;
        private static ObjectLoadingHost Host => IsConfigured ? current.Host :
            throw new InvalidOperationException("Configure an ObjectLoadingHost before using Objects.");
        public static IDisposable Bind(ObjectLoadingHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            if (IsConfigured) throw new InvalidOperationException("A default object loading host is already registered.");
            return current = new Registration(host);
        }
        public static Task<ObjectLoadResult> LoadAsync(ObjectKey key, string url, Transform parent = null,
            CancellationToken cancellationToken = default) => Host.LoadAsync(key, url, parent, cancellationToken);
        public static void Unload(ObjectKey key) => Host.Unload(key);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { current = null; }
        private sealed class Registration : IDisposable
        {
            public Registration(ObjectLoadingHost host) { Host = host; }
            public ObjectLoadingHost Host { get; }
            public void Dispose() { if (ReferenceEquals(current, this)) current = null; }
        }
    }
}
