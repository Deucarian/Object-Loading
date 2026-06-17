# Direct URL AssetBundle Loader Sample

Open `DirectUrlAssetBundleLoaderSample.unity`, enter a direct AssetBundle URL, optionally enter a bearer token or one custom header, then press Load.

The sample does not use API. It passes the final URL and auth data directly into `ObjectLoadRequest`, displays progress/status, prints Object Loading state and loaded-object metadata, and unloads the returned handle when requested.

Install `com.deucarian.diagnostics` when you want this Object Loading state to appear inside shared Deucarian diagnostic snapshots. When Diagnostics is present, Object Loading's optional diagnostics assembly exposes an explicit provider registration helper.

Example registration from a loader that owns an `ObjectLoadingPipeline`:

```csharp
using System;
using Deucarian.ObjectLoading.Diagnostics;

private IDisposable diagnosticsRegistration;

private void OnEnable()
{
    diagnosticsRegistration = ObjectLoadingDiagnostics.Register(pipeline);
}

private void OnDisable()
{
    diagnosticsRegistration?.Dispose();
    diagnosticsRegistration = null;
}
```

Place registration code in an assembly that references `Deucarian.ObjectLoading.Diagnostics`; that optional assembly is compiled only when `com.deucarian.diagnostics` is installed. Object Loading does not create the Diagnostics overlay or window.
