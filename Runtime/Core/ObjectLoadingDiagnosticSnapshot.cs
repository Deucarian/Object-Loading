using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Deucarian.ObjectLoading
{
    public interface IObjectLoadingDiagnosticsSource
    {
        ObjectLoadingDiagnosticSnapshot CreateDiagnosticSnapshot();
    }

    public sealed class ObjectLoadingDiagnosticSnapshot
    {
        public ObjectLoadingDiagnosticSnapshot()
        {
            ActiveComponents = new ObjectLoadingComponentInfo();
            LoadedObjects = new List<ObjectLoadingLoadedObjectInfo>();
        }

        [JsonProperty("is_loading")]
        public bool IsLoading { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("source_type")]
        public string SourceType { get; set; }

        [JsonProperty("current_phase")]
        public ObjectLoadPhase CurrentPhase { get; set; }

        [JsonProperty("current_stage")]
        public string CurrentStage { get; set; }

        [JsonProperty("progress")]
        public float Progress { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("elapsed_ms")]
        public long ElapsedMs { get; set; }

        [JsonProperty("current_request")]
        public ObjectLoadRequestDebugSnapshot CurrentRequest { get; set; }

        [JsonProperty("current_progress")]
        public ObjectLoadProgress CurrentProgress { get; set; }

        [JsonProperty("latest_load_result")]
        public ObjectLoadResult LatestLoadResult { get; set; }

        [JsonProperty("last_error")]
        public ObjectLoadError LastError { get; set; }

        [JsonProperty("current_error")]
        public ObjectLoadError CurrentError { get; set; }

        [JsonProperty("latest_telemetry")]
        public ObjectLoadTelemetry LatestTelemetry { get; set; }

        [JsonProperty("object_metadata")]
        public ObjectDiagnosticsReport ObjectMetadata { get; set; }

        [JsonProperty("active_components")]
        public ObjectLoadingComponentInfo ActiveComponents { get; set; }

        [JsonProperty("loaded_objects")]
        public List<ObjectLoadingLoadedObjectInfo> LoadedObjects { get; set; }
    }

    public sealed class ObjectLoadingComponentInfo
    {
        [JsonProperty("source_resolver")]
        public string SourceResolver { get; set; }

        [JsonProperty("source_content_loader")]
        public string SourceContentLoader { get; set; }

        [JsonProperty("downloader")]
        public string Downloader { get; set; }

        [JsonProperty("content_loader")]
        public string ContentLoader { get; set; }

        [JsonProperty("instantiator")]
        public string Instantiator { get; set; }

        [JsonProperty("object_metadata_collector")]
        public string ObjectMetadataCollector { get; set; }
    }

    public sealed class ObjectLoadingLoadedObjectInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("active_self")]
        public bool ActiveSelf { get; set; }

        [JsonProperty("scene")]
        public string Scene { get; set; }

        public static ObjectLoadingLoadedObjectInfo FromGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            return new ObjectLoadingLoadedObjectInfo
            {
                Name = gameObject.name,
                ActiveSelf = gameObject.activeSelf,
                Scene = gameObject.scene.IsValid() ? gameObject.scene.name : null
            };
        }
    }
}
