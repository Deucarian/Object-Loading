# Deucarian Object Loading

## What this is

`com.deucarian.object-loading` is a small Unity UPM package for loading AssetBundle-based object or scene content at runtime.

The package owns only the generic loading pipeline:

`ObjectLoadRequest -> source resolver -> source content loader -> instantiator -> diagnostics -> result/handle`

It is designed for callers that already know the final AssetBundle URL.

Current package version: `1.2.1`.

## When to use it

- You need to load direct AssetBundle URLs, local AssetBundle files, or explicit raw AssetBundle bytes.
- You need a runtime-safe load pipeline with progress, telemetry, diagnostics, and cleanup handles.
- You need optional bearer token/custom header support for direct bundle sources.
- You want optional Diagnostics integration without making Diagnostics a hard dependency.

## When not to use it

- Do not use Object Loading for backend project/object/version lookup; use the API integration package for that.
- Do not use it for glTF loading, Addressables integration, render pipeline repair, or material remapping.
- Do not add Diagnostics as a required dependency for the core loading package.
- Do not copy Unity object cleanup helpers here; production cleanup uses `com.deucarian.common`.

## What it does

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

## What it does not do yet

- No glTF loading.
- No Addressables integration.
- No backend project/object/version lookup.
- No API dependency in the core package.
- No Diagnostics package dependency in the core package.
- No material remapping, shader replacement, or render pipeline fixes.
- No ServiceLocator.
- No cache eviction policy or storage quota management.

Keep backend-specific source selection outside this core package. Pass the resolved URL, token, and headers into `ObjectLoadRequest`.

## Install

Stable:

```json
"com.deucarian.object-loading": "https://github.com/Deucarian/Object-Loading.git#main"
```

Development:

```json
"com.deucarian.object-loading": "https://github.com/Deucarian/Object-Loading.git#develop"
```

Dependencies:

- `com.deucarian.common`: approved transient Unity object cleanup when loaded objects are released.
- `com.deucarian.logging`: package diagnostics and category-based loading telemetry.
- `com.unity.nuget.newtonsoft-json`: structured debug snapshots and metadata serialization.

Diagnostics support is optional through version-defined assemblies, and API-backed loading is supplied by the separate `com.deucarian.object-loading.api-integration` package.

## Unity compatibility

Requires Unity 2021.3 or newer.

## Logging

This package uses `com.deucarian.logging`.

Object Loading uses stable package categories: `ObjectLoading`, `ObjectLoading.Downloader`, `ObjectLoading.Loader`, `ObjectLoading.Instantiation`, and `ObjectLoading.Diagnostics`. Configure Deucarian Logging filters by category and level to isolate download, load, instantiation, or object metadata output.

## 60-second quick start

### Direct URL loading

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

## Public API map

- `ObjectLoadRequest`: request model for URL/file/raw-byte loading, headers, auth, cache metadata, platform query, transform placement, and progress.
- `ObjectLoadingPipeline`: orchestrates source resolution, content loading, instantiation, diagnostics, progress, telemetry, and latest-state snapshots.
- `ObjectLoadResult`, `ObjectLoadError`, and `ObjectLoadTelemetry`: result, failure, timing, cache, and metadata outputs.
- `IObjectLoadHandle`: cleanup handle that unloads instantiated content and the AssetBundle.
- `DefaultObjectDiagnostics` and `ObjectDiagnosticsReport`: package-owned object, scene, renderer, material, shader, and warning reporting.
- `ObjectLoadingDiagnostics.Register(...)`: optional Diagnostics provider registration when `com.deucarian.diagnostics` is installed.

## Integrations

Works with:

- `com.deucarian.common` for transient Unity object cleanup,
- `com.deucarian.logging` for package diagnostics,
- optional `com.deucarian.diagnostics` through guarded/version-defined assemblies,
- `com.deucarian.object-loading.api-integration` for API-backed source selection.

Does not own:

- backend lookup/download policy,
- Diagnostics UI,
- package installation,
- renderer/material repair,
- Addressables or glTF loading.

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

## Validation

Run the shared package validator from the repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Run the package's EditMode tests in Unity after code or assembly definition changes. Tests cover request modeling, URL/source resolution, result handling, cleanup handles, and diagnostics behavior.

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Architecture / Contributor Notes

- [AGENTS.md](AGENTS.md) contains repository-specific ownership and Codex guidance.
- Deucarian architecture rules live in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md).
- Capability ownership is tracked in [CAPABILITY_OWNERSHIP.md](https://github.com/Deucarian/Package-Registry/blob/develop/CAPABILITY_OWNERSHIP.md).

## License

See [LICENSE.md](LICENSE.md).
