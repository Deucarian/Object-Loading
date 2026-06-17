using System;
using Deucarian.Diagnostics;

namespace Deucarian.ObjectLoading.Diagnostics
{
    public static class ObjectLoadingDiagnostics
    {
        public static IDisposable Register(IObjectLoadingDiagnosticsSource source)
        {
            return DiagnosticProviderRegistry.Register(new ObjectLoadingDiagnosticProvider(source));
        }
    }
}
