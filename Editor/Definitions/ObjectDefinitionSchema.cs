using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.ObjectLoading.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.ObjectLoading.Editor.Definitions
{
    public sealed class ObjectDefinitionSchema : DeucarianSerializedDefinitionSchema<ObjectDefinitionAsset, ObjectDefinitionSpec>
    {
        public override string Id => "objects";
        public override string DisplayName => "Loadable objects";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            ((ObjectDefinitionAsset)asset).Validate();
        }
        public override void RefreshCatalog(bool validateOnly = false) { ProjectDefinitionCatalogAuthoring.Refresh(validateOnly); }
        [MenuItem("Assets/Create/Deucarian/Object Loading/Object Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new ObjectDefinitionSchema(), "NewObject"); }
    }
}
