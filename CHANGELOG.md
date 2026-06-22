# Changelog

## 1.1.1 - 2026-06-22

- Documented the current package metadata and dependency versions for the 1.1.1 Object Loading package line.
- Clarified that Logging is a runtime dependency, Diagnostics remains optional through version-defined assemblies, and API-backed loading lives in Object Loading API Integration.

## 1.1.0

- Moved broad runtime diagnostics overlay, snapshot JSON export, and snapshot console formatting out of Object Loading.
- Added explicit pipeline state access for current phase/stage/progress, latest result, last error, timing telemetry, active pipeline components, and loaded object metadata.
- Added a failure-result overload that preserves available telemetry.
- Updated the direct URL sample to display Object Loading state data without depending on a diagnostics overlay.
- Added an optional Object Loading diagnostics provider assembly that compiles only when `com.deucarian.diagnostics` is installed.
- Documented that Deucarian Diagnostics integration is optional and does not add a hard package dependency.

## 0.4.1

- Aligned the Newtonsoft Json dependency to `3.2.2` with the API packages.
- Added package license metadata.
- Updated README structure with overview, installation, usage, tests, and license sections.
- Standardized package logging on com.deucarian.logging.
- Added `ObjectLoadingLog` package categories and routed diagnostics logging through Deucarian Logging.

## 0.4.0

- Added reusable runtime Object Loading diagnostics overlay.
- Added diagnostics JSON snapshot/copy support.
- Added package-level diagnostics logging helpers.
- Updated the direct URL sample to demonstrate the reusable overlay.

## 0.3.0

- Added structured load phases to progress callbacks.
- Added elapsed time and telemetry snapshots to progress updates.
- Added renderer, material, missing shader, and pink material counts to load telemetry.
- Added completed and failed progress phases for caller diagnostics overlays.

## 0.2.0

- Replaced the default byte-array AssetBundle path with source-centric loading.
- Added remote URL loading through `UnityWebRequestAssetBundle.GetAssetBundle`.
- Added local file loading through `AssetBundle.LoadFromFileAsync`.
- Preserved raw-byte loading through `AssetBundle.LoadFromMemoryAsync`.
- Added cache metadata fields to `ObjectLoadRequest`.
- Added timing and cache telemetry to `ObjectLoadResult`.

## 0.1.0

- Added initial UPM package metadata.
- Added direct URL AssetBundle loading pipeline.
- Added runtime request, result, error, progress, source, diagnostics, and handle types.
- Added source resolver, UnityWebRequest downloader, AssetBundle content loader, AssetBundle instantiator, diagnostics, and pipeline implementations.
- Added EditMode tests for pure request, URL, result, cleanup, and diagnostics behavior.
- Added direct URL loading sample.
