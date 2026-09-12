using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.ObjectLoading.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.ObjectLoading.Editor.Definitions
{
    public sealed class ObjectKeySource : DeucarianAssetKeySource<ObjectDefinitionAsset>
    {
        public override Type KeyType => typeof(ObjectKey);
        public override Type DefinitionSetAttribute => typeof(ObjectKeySetAttribute);
        public override string GeneratedClassName => "ProjectObjects";
        protected override DeucarianKeyChoice ReadDefinition(ObjectDefinitionAsset asset) => new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
