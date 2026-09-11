# Simple usage

Add ObjectLoadingHost once. Its default factory uses the existing direct URL AssetBundle pipeline. Configure a custom pipeline factory once if your application needs different source/content adapters. Each ID owns its request and loaded handle. Loading the same ID replaces it; Unload cancels it or disposes its handle. Disabling the host releases every owned operation and handle. LoadAsync returns ObjectLoadResult; its handle is borrowed from the host, so release through Unload. Calls require Unity's main thread. For independent scopes use a host reference instead of Objects.

Import the **Simple Usage** sample from Unity Package Manager. Its caller script is:

Definition fields now use named, domain-specific keys. Select an existing definition from the Inspector dropdown or pass the same named key in code. Declare each project key once in a marked key set; ordinary caller methods do not accept raw IDs. Generated keys for asset-authored definitions require no asset reference in the caller. Owner-issued selection and row handles represent runtime instances.

```csharp
using UnityEngine;

namespace Deucarian.ObjectLoading.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private ObjectKey preview = ObjectKeys.Preview;

        public System.Threading.Tasks.Task Load(string url) => Objects.LoadAsync(preview, url, transform);
        public void Unload() => Objects.Unload(preview);
    }
}
```
