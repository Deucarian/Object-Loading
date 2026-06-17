using System;
using Deucarian.Diagnostics;

namespace Deucarian.ObjectLoading.Diagnostics
{
    public sealed class ObjectLoadingDiagnosticProvider : IDiagnosticProvider
    {
        private readonly IObjectLoadingDiagnosticsSource source;

        public ObjectLoadingDiagnosticProvider(IObjectLoadingDiagnosticsSource source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public string ProviderId
        {
            get { return "deucarian.object-loading"; }
        }

        public string DisplayName
        {
            get { return "Deucarian Object Loading"; }
        }

        public void Collect(DiagnosticReportBuilder builder)
        {
            ObjectLoadingDiagnosticSnapshot snapshot = source.CreateDiagnosticSnapshot();
            DiagnosticSection section = builder.AddSection(ProviderId, DisplayName);

            if (snapshot == null)
            {
                section.AddItem("state", "State", "Unavailable", DiagnosticSeverity.Warning);
                return;
            }

            DiagnosticSeverity summary = GetSummarySeverity(snapshot);
            section.AddItem("state", "State", snapshot.IsLoading ? "Loading" : "Idle", summary);
            section.AddItem("object", "Object", FormatValue(snapshot.DisplayName), summary);
            section.AddItem("phase", "Phase", snapshot.CurrentPhase.ToString(), ToSeverity(snapshot.CurrentPhase));
            section.AddItem("stage", "Stage", FormatValue(snapshot.CurrentStage), ToSeverity(snapshot.CurrentPhase));
            section.AddItem("progress", "Progress", FormatPercent(snapshot.Progress), ToSeverity(snapshot.CurrentPhase));
            section.AddItem("elapsed_ms", "Elapsed", snapshot.ElapsedMs + " ms", DiagnosticSeverity.Info);
            section.AddItem("message", "Message", FormatValue(snapshot.Message), summary);
            section.AddItem("source", "Source", FormatValue(snapshot.SourceType), DiagnosticSeverity.Info);

            AddResultItems(section, snapshot);
            AddErrorItems(section, snapshot);
            AddComponentItems(section, snapshot.ActiveComponents);
            AddTimingItems(section, snapshot.LatestTelemetry);
            AddMetadataItems(section, snapshot);
        }

        private static void AddResultItems(DiagnosticSection section, ObjectLoadingDiagnosticSnapshot snapshot)
        {
            if (snapshot.LatestLoadResult == null)
            {
                section.AddItem("latest_result", "Latest Result", snapshot.IsLoading ? "Loading" : "None", DiagnosticSeverity.Info);
                return;
            }

            section.AddItem(
                "latest_result",
                "Latest Result",
                snapshot.LatestLoadResult.Succeeded ? "Succeeded" : "Failed",
                snapshot.LatestLoadResult.Succeeded ? DiagnosticSeverity.Success : DiagnosticSeverity.Error,
                snapshot.LatestLoadResult.Message);

        }

        private static void AddErrorItems(DiagnosticSection section, ObjectLoadingDiagnosticSnapshot snapshot)
        {
            AddErrorItem(section, "current_error", "Current Error", snapshot.CurrentError);
            AddErrorItem(section, "last_error", "Last Error", snapshot.LastError);
        }

        private static void AddErrorItem(DiagnosticSection section, string key, string label, ObjectLoadError error)
        {
            if (error == null)
            {
                section.AddItem(key, label, "none", DiagnosticSeverity.Info);
                return;
            }

            section.AddItem(
                key,
                label,
                error.Code.ToString(),
                DiagnosticSeverity.Error,
                error.Message);
        }

        private static void AddTimingItems(DiagnosticSection section, ObjectLoadTelemetry telemetry)
        {
            if (telemetry == null)
            {
                section.AddItem("timings", "Timings", "Unavailable", DiagnosticSeverity.Info);
                return;
            }

            section.AddItem("download_time_ms", "Download", telemetry.DownloadTimeMs + " ms", DiagnosticSeverity.Info);
            section.AddItem("bundle_load_time_ms", "Bundle", telemetry.BundleLoadTimeMs + " ms", DiagnosticSeverity.Info);
            section.AddItem("instantiate_time_ms", "Instantiate", telemetry.InstantiateTimeMs + " ms", DiagnosticSeverity.Info);
            section.AddItem("total_time_ms", "Total", telemetry.TotalTimeMs + " ms", DiagnosticSeverity.Info);
            section.AddItem("bytes_received", "Bytes", telemetry.BytesReceived.ToString(), DiagnosticSeverity.Info);
            section.AddItem("asset_count", "Assets", telemetry.AssetCount.ToString(), DiagnosticSeverity.Info);
            section.AddItem("scene_count", "Scenes", telemetry.SceneCount.ToString(), DiagnosticSeverity.Info);
            section.AddItem("renderer_count", "Renderers", telemetry.RendererCount.ToString(), telemetry.RendererCount > 0 ? DiagnosticSeverity.Success : DiagnosticSeverity.Info);
            section.AddItem("material_count", "Materials", telemetry.MaterialCount.ToString(), DiagnosticSeverity.Info);
            section.AddItem("missing_shader_material_count", "Missing Shaders", telemetry.MissingShaderMaterialCount.ToString(), telemetry.MissingShaderMaterialCount > 0 ? DiagnosticSeverity.Warning : DiagnosticSeverity.Success);
            section.AddItem("pink_material_count", "Pink Materials", telemetry.PinkMaterialCount.ToString(), telemetry.PinkMaterialCount > 0 ? DiagnosticSeverity.Warning : DiagnosticSeverity.Success);
            section.AddItem("strategy", "Strategy", FormatValue(telemetry.LoadStrategy), DiagnosticSeverity.Info);
            section.AddItem("cache_status", "Cache", FormatValue(telemetry.CacheStatus), DiagnosticSeverity.Info);
        }

        private static void AddComponentItems(DiagnosticSection section, ObjectLoadingComponentInfo components)
        {
            if (components == null)
            {
                return;
            }

            section.AddItem("source_resolver", "Source Resolver", FormatValue(components.SourceResolver), DiagnosticSeverity.Info);
            section.AddItem("source_content_loader", "Source Content Loader", FormatValue(components.SourceContentLoader), DiagnosticSeverity.Info);
            section.AddItem("downloader", "Downloader", FormatValue(components.Downloader), DiagnosticSeverity.Info);
            section.AddItem("content_loader", "Content Loader", FormatValue(components.ContentLoader), DiagnosticSeverity.Info);
            section.AddItem("instantiator", "Instantiator", FormatValue(components.Instantiator), DiagnosticSeverity.Info);
            section.AddItem("object_metadata_collector", "Metadata Collector", FormatValue(components.ObjectMetadataCollector), DiagnosticSeverity.Info);
        }

        private static void AddMetadataItems(DiagnosticSection section, ObjectLoadingDiagnosticSnapshot snapshot)
        {
            ObjectDiagnosticsReport metadata = snapshot.ObjectMetadata;
            if (metadata != null && metadata.Warnings != null)
            {
                for (int i = 0; i < metadata.Warnings.Count; i++)
                {
                    section.AddItem("metadata_warning_" + (i + 1), "Warning", metadata.Warnings[i], DiagnosticSeverity.Warning);
                }
            }

            AddLoadedObjectItems(section, snapshot.LoadedObjects);
        }

        private static void AddLoadedObjectItems(DiagnosticSection section, System.Collections.Generic.List<ObjectLoadingLoadedObjectInfo> loadedObjects)
        {
            if (loadedObjects == null)
            {
                return;
            }

            for (int i = 0; i < loadedObjects.Count; i++)
            {
                ObjectLoadingLoadedObjectInfo loadedObject = loadedObjects[i];
                if (loadedObject != null)
                {
                    section.AddItem("loaded_object_" + (i + 1), "Loaded Object", FormatValue(loadedObject.Name), DiagnosticSeverity.Info);
                }
            }
        }

        private static DiagnosticSeverity GetSummarySeverity(ObjectLoadingDiagnosticSnapshot snapshot)
        {
            if (snapshot.LastError != null ||
                snapshot.CurrentPhase == ObjectLoadPhase.Failed ||
                (snapshot.LatestLoadResult != null && !snapshot.LatestLoadResult.Succeeded))
            {
                return DiagnosticSeverity.Error;
            }

            if (snapshot.LatestLoadResult != null && snapshot.LatestLoadResult.Succeeded)
            {
                return DiagnosticSeverity.Success;
            }

            return DiagnosticSeverity.Info;
        }

        private static DiagnosticSeverity ToSeverity(ObjectLoadPhase phase)
        {
            if (phase == ObjectLoadPhase.Failed)
            {
                return DiagnosticSeverity.Error;
            }

            if (phase == ObjectLoadPhase.Completed)
            {
                return DiagnosticSeverity.Success;
            }

            return DiagnosticSeverity.Info;
        }

        private static string FormatPercent(float value)
        {
            double clamped = Math.Max(0d, Math.Min(1d, value));
            return clamped.ToString("P0");
        }

        private static string FormatValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "none" : value;
        }
    }
}
