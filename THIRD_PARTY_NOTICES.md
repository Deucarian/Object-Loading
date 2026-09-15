# Third-party notices

This notice describes the dependency and distribution inventory for `com.deucarian.object-loading` `1.2.1`. It does not replace the repository's [MIT license](LICENSE.md), and it does not grant rights to software supplied separately.

## Review basis

The reviewed baseline is `origin/main` commit `3e116919b6e03a3ac959cee1f96b4fef2b31bc60`. Its `npm pack --dry-run` inventory contained 91 package files. The tracked and packed inventories were checked for common vendor/third-party directories, compiled binaries and archives, Git submodules, Git LFS pointers, separate license markers, and media/font assets.

That inventory identified no files marked or located as vendored third-party source, no compiled binary/archive candidates, no submodules, no LFS pointers, and no media/font asset candidates. The dependencies below are resolved separately by Unity Package Manager; they are not copied into this repository's package archive.

## Deucarian dependencies (not third-party)

| Package | Version / mode | Relationship | License |
|---|---:|---|---|
| `com.deucarian.common` | `0.1.0` | Direct package dependency | [MIT](https://github.com/Deucarian/Common/blob/main/LICENSE.md) |
| `com.deucarian.logging` | `1.0.1` | Direct package dependency | [MIT](https://github.com/Deucarian/Logging/blob/main/LICENSE.md) |
| `com.deucarian.diagnostics` | Optional, version-defined | Not installed by `package.json`; integration activates only when present | [MIT](https://github.com/Deucarian/Diagnostics/blob/main/LICENSE.md) |

The `com.deucarian.common` version above records the current manifest exactly; it is intentionally not normalized to a newer catalog version in this metadata-only change.

## External package dependencies

| Package | Version | Provider / purpose | Applicable terms |
|---|---:|---|---|
| `com.unity.nuget.newtonsoft-json` | `3.2.2` | Unity package wrapping Newtonsoft.Json `13.0.2` | [Unity package license](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/license/LICENSE.html); [embedded MIT components](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/license/Third%20Party%20Notices.html) |

The Newtonsoft package's official third-party notice identifies Newtonsoft.Json, Json.Net.Unity3D, Newtonsoft.Json-for-Unity, and com.newtonsoft.json as MIT-licensed components. Their license text travels with the separately resolved Unity package rather than this repository.

## Host platform

The manifest requires Unity `2021.3`. Unity is not included in this package and is governed by the applicable [Unity Editor Software Terms](https://unity.com/legal/editor-terms-of-service/software).

Re-run the inventory and update this notice whenever dependencies or distributed content change.
