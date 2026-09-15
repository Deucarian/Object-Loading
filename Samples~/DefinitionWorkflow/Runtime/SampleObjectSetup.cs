using System;
using System.Collections;
using UnityEngine;
namespace Deucarian.ObjectLoading.Samples.DefinitionWorkflow
{
    [DefaultExecutionOrder(-2000)]
    public sealed class SampleObjectSetup : MonoBehaviour
    {
        [SerializeField] private ObjectLoadingHost host;
        [SerializeField] private GameObject prefab;
        private void Awake() => host.Configure(() => new LocalPrefabPipeline(prefab));
        // Offline fixture: the real host still owns request cancellation and the resulting handle.
        private sealed class LocalPrefabPipeline : IObjectLoadingPipeline
        {
            private readonly GameObject prefab;
            public LocalPrefabPipeline(GameObject prefab) { this.prefab = prefab; }
            public IEnumerator LoadAsync(ObjectLoadRequest request, Action<ObjectLoadResult> completed)
            {
                yield return null;
                request.CancellationToken.ThrowIfCancellationRequested();
                var instance = UnityEngine.Object.Instantiate(prefab, new Vector3(2, 0, 0), Quaternion.identity, request.Parent);
                completed(ObjectLoadResult.Success("Loaded local fixture.", new ObjectLoadHandle(instance, null)));
            }
            public void UnloadLast() { }
        }
    }
}
