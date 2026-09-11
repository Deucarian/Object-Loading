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
