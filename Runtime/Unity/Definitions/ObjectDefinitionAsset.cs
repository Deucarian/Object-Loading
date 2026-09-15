using System;

using UnityEngine;

namespace Deucarian.ObjectLoading.Unity
{
    /// <summary>Reusable defaults for code calls and serialized typed keys; runtime state remains in the core service.</summary>
    public sealed class ObjectDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string url = string.Empty;
        public string Id => id;
        public string DisplayName => displayName;
        public ObjectKey Key => new AssetKey(id);
        public string Url => url;
        public void Validate() { if (string.IsNullOrWhiteSpace(url)) throw new InvalidOperationException("Assign the object URL in Definitions before loading it."); }
        private sealed class AssetKey : ObjectKey { public AssetKey(string value) : base(value) { } }
    }
}
