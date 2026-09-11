using System;
using System.Linq;
using UnityEngine;
namespace Deucarian.ObjectLoading.Unity
{
    public sealed class ObjectDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/ObjectDefinitionCatalog";
        [SerializeField] private ObjectDefinitionAsset[] definitions = Array.Empty<ObjectDefinitionAsset>();
        public static ObjectDefinitionCatalog LoadProject() => Resources.Load<ObjectDefinitionCatalog>(ResourcePath) ?? throw new InvalidOperationException("Create a Object definition in Definitions before using its catalog.");
        public ObjectDefinitionAsset Get(ObjectKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key), "Select a Object key in the Inspector or pass a generated key.");
            var matches = definitions.Where(x => x != null && x.Id == key.Id).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("The Object catalog needs exactly one definition for '" + key.Id + "'. Synchronize Definitions before using it.");
            matches[0].Validate(); return matches[0];
        }
    }
}
