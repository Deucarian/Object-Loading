using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.ObjectLoading.Unity;
using UnityEditor;
namespace Deucarian.ObjectLoading.Editor.Definitions
{
    internal static class ProjectDefinitionCatalogAuthoring
    {
        internal static void Refresh(bool validateOnly = false)
        {
            var definitions = AssetDatabase.FindAssets("t:ObjectDefinitionAsset", new[] { "Assets" }).Select(x => AssetDatabase.LoadAssetAtPath<ObjectDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x))).Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("Object definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<ObjectDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/ObjectDefinitionCatalog.asset", "definitions", definitions, validateOnly);
        }
    }
}
