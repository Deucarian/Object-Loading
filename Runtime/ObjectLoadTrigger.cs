using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
namespace Deucarian.ObjectLoading
{
    public sealed class ObjectLoadTrigger : MonoBehaviour
    {
        [SerializeField] private ObjectLoadingHost host;
        [SerializeField] private ObjectKey key;
        [SerializeField] private Transform parent;
        [SerializeField] private UnityEvent loaded = new UnityEvent();
        [SerializeField] private UnityEvent failed = new UnityEvent();
        public ObjectLoadResult LastResult { get; private set; }
        private ObjectLoadingHost Host => host != null ? host : throw new InvalidOperationException("Assign an ObjectLoadingHost to this ObjectLoadTrigger.");
        public Task<ObjectLoadResult> LoadAsync() => Host.LoadAsync(key, parent);
        public async void Load()
        {
            LastResult = await LoadAsync();
            if (this == null) return;
            if (LastResult.Succeeded) loaded.Invoke(); else failed.Invoke();
        }
        public void Unload() => Host.Unload(key);
    }
}
