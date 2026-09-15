using System;
using UnityEngine;

namespace Deucarian.ObjectLoading.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class ObjectLoadingWorkflow : MonoBehaviour
    {
        [SerializeField] private ObjectKey key;
        [SerializeField] private ObjectLoadTrigger trigger;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public async void Load() { var result = await Objects.LoadAsync(key); status = result.Succeeded ? "Loaded the sample object using the local fixture pipeline." : "Load failed."; }
        public void LoadComponent() { trigger.Load(); status = "Load requested through ObjectLoadTrigger."; }
        public void Unload() { Objects.Unload(key); status = "The host disposed the loaded object handle."; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("Object-Loading — definition workflow");
            GUILayout.Label("The typed definition supplies a default URL. This offline scene substitutes a local prefab pipeline; the real loading host still owns cancellation, replacement and cleanup.");
            GUILayout.Space(12);
            if (GUILayout.Button("Load with C#", GUILayout.Height(32))) { try { Load(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Load from component", GUILayout.Height(32))) { try { LoadComponent(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Unload", GUILayout.Height(32))) { try { Unload(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
