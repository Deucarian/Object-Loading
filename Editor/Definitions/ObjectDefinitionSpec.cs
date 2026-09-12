using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.ObjectLoading.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.ObjectLoading.Editor.Definitions
{
    [Serializable]
    public sealed class ObjectDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("url")] public string Url = string.Empty;
    }
}
