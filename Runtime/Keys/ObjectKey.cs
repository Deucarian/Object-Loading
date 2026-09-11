using System;
using UnityEngine;

namespace Deucarian.ObjectLoading
{
    /// <summary>A declared Object identity. Reuse a named definition or select it in the Inspector.</summary>
    [Serializable]
    public class ObjectKey : IEquatable<ObjectKey>
    {
        [SerializeField] private string definitionId;

        /// <summary>For central definition sets and generated declarations; ordinary callers reuse those keys.</summary>
        protected ObjectKey(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim())
                throw new ArgumentException("A ObjectKey definition needs a non-empty stable ID without surrounding whitespace.", nameof(id));
            definitionId = id;
        }

        public string Id => !string.IsNullOrWhiteSpace(definitionId) ? definitionId :
            throw new InvalidOperationException("No ObjectKey is selected. Select an existing definition in the Inspector or assign a named key from a ObjectKeySet declaration.");
        public bool Equals(ObjectKey other) => other != null && string.Equals(definitionId, other.definitionId, StringComparison.Ordinal);
        public override bool Equals(object other) => other is ObjectKey key && Equals(key);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(definitionId ?? string.Empty);
        public override string ToString() => definitionId ?? string.Empty;
    }
}
