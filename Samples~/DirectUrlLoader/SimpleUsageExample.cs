using UnityEngine;

namespace Deucarian.ObjectLoading.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        public System.Threading.Tasks.Task Load(string url) => Objects.LoadAsync("preview.model", url, transform);
        public void Unload() => Objects.Unload("preview.model");
    }
}
