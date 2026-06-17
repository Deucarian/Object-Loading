# Direct URL AssetBundle Loader Sample

Open `DirectUrlAssetBundleLoaderSample.unity`, enter a direct AssetBundle URL, optionally enter a bearer token or one custom header, then press Load.

The sample does not use API. It passes the final URL and auth data directly into `ObjectLoadRequest`, displays progress/status, prints Object Loading state and loaded-object metadata, and unloads the returned handle when requested.

Install `com.deucarian.diagnostics` when you want this Object Loading state to appear inside shared Deucarian diagnostic snapshots. When Diagnostics is present, Object Loading's optional diagnostics assembly exposes an explicit provider registration helper.
