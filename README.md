# Deucarian Object Loading

## Overview

`com.deucarian.object-loading` is a small Unity UPM package for loading AssetBundle-based object or scene content at runtime.

The package owns only the generic loading pipeline:

`ObjectLoadRequest -> source resolver -> source content loader -> instantiator -> diagnostics -> result/handle`

It is designed for callers that already know the final AssetBundle URL.

## What It Does

- Loads direct AssetBundle URLs, local AssetBundle files, or explicit raw AssetBundle bytes.
- Adds optional request headers and optional bearer token auth.
- Optionally appends a `platform` query, defaulting to `platform=webgl` unless overridden.
- Loads remote bundles with `UnityWebRequestAssetBundle.GetAssetBundle`.
- Loads local files with `AssetBundle.LoadFromFileAsync`.
- Keeps `AssetBundle.LoadFromMemoryAsync` available for explicit raw-byte workflows.
- Supports cache metadata for remote AssetBundle requests.
- Instantiates bundled scenes first by default, or prefab/GameObject assets when no scene is present.
- Applies optional parent, position, rotation, and scale to the loaded root container.
- Returns a cleanup handle that destroys instantiated GameObjects and unloads the AssetBundle.
- Reports diagnostics for assets, scenes, renderers, materials, shaders, and likely shader/material problems.
- Reports timing telemetry for download, bundle load, instantiation, total time, asset count, scene count, and cache status.
- Reports structured loading phases through `ObjectLoadRequest.Progress`.
- Exposes explicit current state, latest result, last error, timing telemetry, active pipeline components, and loaded object metadata for callers and optional integrations.

## What It Does Not Do Yet

- No glTF loading.
- No Addressables integration.
- No backend project/object/version lookup.
- No API dependency in the core package.
- No Diagnostics package dependency in the core package.
- No material remapping, shader replacement, or render pipeline fixes.
- No ServiceLocator.
- No cache eviction policy or storage quota management.

Keep backend-specific source selection outside this core package. Pass the resolved URL, token, and headers into `ObjectLoadRequest`.

## Installation

Add the package to a Unity project through Package Manager using this package folder or a Git URL, then import the optional sample from Package Manager.

The package depends on Deucarian Common, Deucarian Logging, and Unity's Newtonsoft Json package:

```json
"com.deucarian.common": "0.1.0",
"com.deucarian.logging": "0.2.5",
"com.unity.nuget.newtonsoft-json": "3.2.2"
```

`com.deucarian.common` is a runtime dependency for safe transient Unity object cleanup when loaded objects are released. `com.deucarian.logging` is a runtime dependency for package diagnostics and category-based loading telemetry. Unity's Newtonsoft Json package supports structured debug snapshots and metadata serialization. Diagnostics support is optional through version-defined assemblies, and API-backed loading is supplied by the separate `com.deucarian.object-loading.api-integration` package.

## Logging

This package uses `com.deucarian.logging`.

Object Loading uses stable package categories: `ObjectLoading`, `ObjectLoading.Downloader`, `ObjectLoading.Loader`, `ObjectLoading.Instantiation`, and `ObjectLoading.Diagnostics`. Configure Deucarian Logging filters by category and level to isolate download, load, instantiation, or object metadata output.

## Usage

### Direct URL Loading

```csharp
using System.Collections;
using Deucarian.ObjectLoading;
using UnityEngine;

public sealed class ExampleLoader : MonoBehaviour
{
    private readonly ObjectLoadingPipeline _pipeline = new ObjectLoadingPipeline();
    private IObjectLoadHandle _handle;

    public IEnumerator Load(string assetBundleUrl, Transform parent)
    {
        ObjectLoadRequest request = ObjectLoadRequest.FromUrl(assetBundleUrl);
        request.Parent = parent;
        request.DisplayName = "Loaded object";
        request.LoadPreference = ObjectContentLoadPreference.Automatic;

        ObjectLoadResult result = null;
        yield return _pipeline.LoadAsync(request, value => result = value);

        if (result.Succeeded)
        {
            _handle = result.Handle;
            ObjectLoadingLog.Diagnostics.Info(result.Diagnostics.ToText());
        }
        else
        {
            ObjectLoadingLog.Loader.Error(result.Message);
        }
    }

    public void Unload()
    {
        _handle?.Unload();
        _handle = null;
    }
}
```

## Auth Headers

Use the bearer token convenience when the server expects `Authorization: Bearer ...`.

```csharp
ObjectLoadRequest request = ObjectLoadRequest.FromUrl(url);
request.BearerToken = accessToken;
```

Or pass explicit headers:

```csharp
request.AddHeader("Authorization", "Bearer " + accessToken);
request.AddHeader("X-Custom-Header", "value");
```

If both are supplied, the explicit `Authorization` header wins. `ToDebugSnapshotJson()` redacts bearer tokens and sensitive headers.

## Cache Metadata

Remote loads can request Unity AssetBundle caching when a stable version/hash is available:

```csharp
request.CacheMode = ObjectLoadCacheMode.UseUnityCache;
request.CacheKey = "project-832-model-497";
request.CacheHash = "0123456789abcdef0123456789abcdef";
request.Crc = 0;
```

If no cache metadata is supplied, remote URLs still use `UnityWebRequestAssetBundle` without forcing a managed `byte[]`.

## Platform Query

By default, direct URLs receive a `platform` query parameter when one is not already present:

```text
https://example.com/object.bundle -> https://example.com/object.bundle?platform=webgl
```

Override or disable this per request:

```csharp
request.PlatformOverride = "windows";
request.AppendPlatformQuery = false;
```

## Cleanup

The successful `ObjectLoadResult` includes an `IObjectLoadHandle`.

Calling `Unload()`:

- destroys instantiated root GameObjects,
- unloads the AssetBundle with `AssetBundle.Unload(false)`,
- is safe to call more than once.

Object cleanup uses `Deucarian.Common.UnityObjectUtility.DestroySafely`, preserving Unity's deferred `Object.Destroy` behavior in Play Mode and immediate cleanup outside Play Mode.

`ObjectLoadingPipeline.UnloadLast()` is also available for simple callers that want the pipeline to track the latest successful handle.

## Diagnostics

`DefaultObjectDiagnostics` reports:

- loaded asset names,
- bundled scene paths,
- renderer count,
- material count,
- shader names,
- active render pipeline name,
- missing/error shader count,
- likely pink/magenta material count,
- warnings for common wrong platform or render pipeline symptoms.

Diagnostics report facts and warnings only. They do not change materials or shaders.

`ObjectLoadRequest.Progress` reports structured phases:

- `ResolvingSource`
- `Downloading`
- `LoadingBundle`
- `DiscoveringContent`
- `Instantiating`
- `Diagnostics`
- `Completed`
- `Failed`

Progress updates include normalized progress, elapsed milliseconds when available, bytes received, and the latest `ObjectLoadTelemetry` snapshot.

The pipeline exposes explicit state data when a project wants package-level developer visibility without adding another dependency:

```csharp
yield return pipeline.LoadAsync(
    request,
    result => ObjectLoadingLog.General.Info(result.Message));

ObjectLoadingDiagnosticSnapshot state = pipeline.CreateDiagnosticSnapshot();
ObjectLoadResult latest = pipeline.LatestLoadResult;
ObjectLoadError lastError = pipeline.LastError;
ObjectLoadTelemetry timings = pipeline.LatestTelemetry;
```

Snapshot data includes current phase/stage/progress, latest load result, last error, timing telemetry, active resolver/loader/instantiator names where available, loaded object names, and loaded content metadata where available.

Object Loading does not depend on `com.deucarian.diagnostics`. When Diagnostics is installed, the optional `Deucarian.ObjectLoading.Diagnostics` assembly compiles and provides an explicit provider registration helper:

```csharp
using System;
using Deucarian.ObjectLoading.Diagnostics;

IDisposable registration = ObjectLoadingDiagnostics.Register(pipeline);
```

Keep and dispose the returned registration with the lifetime of the pipeline or loader that owns it. Re-registering the same source is idempotent and does not add duplicate providers.

That provider appears as an Object Loading section inside the shared Deucarian Diagnostics runtime overlay and editor window. Object Loading does not open or auto-spawn those views.

## Samples

Import `Direct URL AssetBundle Loader` from Package Manager. The sample scene provides:

- direct URL input,
- optional bearer token input,
- optional custom header input,
- load/unload buttons,
- status text,
- Object Loading state and loaded-object metadata output.

## Tests

Run the package's EditMode tests in Unity. Tests cover request modeling, URL/source resolution, result handling, cleanup handles, and diagnostics behavior.

## License

See [LICENSE.md](LICENSE.md).
