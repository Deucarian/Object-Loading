using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.ObjectLoading.Editor
{
    [CustomPropertyDrawer(typeof(ObjectKey), true)]
    public sealed class ObjectKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(ObjectKey);
        public override Type DefinitionSetAttribute => typeof(ObjectKeySetAttribute);
        public override string SetupHint => "Select an existing ObjectKey; declare reusable keys once in a [ObjectKeySet] class.";
    }
}
