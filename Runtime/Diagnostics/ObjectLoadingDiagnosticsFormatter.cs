using System.Collections.Generic;

namespace Deucarian.ObjectLoading
{
    internal static class ObjectLoadingDiagnosticsFormatter
    {
        public static string FormatSnapshot(ObjectLoadingDiagnosticsSnapshot snapshot)
        {
            ObjectLoadTelemetry telemetry = snapshot.Telemetry;
            return "Diagnostics snapshot: " +
                   "display_name=" + FormatValue(snapshot.DisplayName) +
                   ", source_type=" + FormatValue(snapshot.SourceType) +
                   ", succeeded=" + (snapshot.Succeeded.HasValue ? snapshot.Succeeded.Value.ToString() : "pending") +
                   ", phase=" + snapshot.Phase +
                   ", stage=" + FormatValue(snapshot.Stage) +
                   ", progress=" + snapshot.Progress.ToString("0.00") +
                   ", message=" + FormatValue(snapshot.Message) +
                   ", load_strategy=" + FormatValue(telemetry?.LoadStrategy) +
                   ", download_ms=" + (telemetry?.DownloadTimeMs ?? 0) +
                   ", bundle_load_ms=" + (telemetry?.BundleLoadTimeMs ?? 0) +
                   ", instantiate_ms=" + (telemetry?.InstantiateTimeMs ?? 0) +
                   ", total_ms=" + (telemetry?.TotalTimeMs ?? 0) +
                   ", bytes_received=" + (telemetry?.BytesReceived ?? 0) +
                   ", assets=" + (telemetry?.AssetCount ?? 0) +
                   ", scenes=" + (telemetry?.SceneCount ?? 0) +
                   ", renderers=" + (telemetry?.RendererCount ?? 0) +
                   ", materials=" + (telemetry?.MaterialCount ?? 0) +
                   ", missing_shader_materials=" + (telemetry?.MissingShaderMaterialCount ?? 0) +
                   ", pink_materials=" + (telemetry?.PinkMaterialCount ?? 0) +
                   ", cache_mode=" + (telemetry != null ? telemetry.CacheMode.ToString() : "Default") +
                   ", cache_status=" + FormatValue(telemetry?.CacheStatus);
        }

        public static string FormatDiagnostics(ObjectDiagnosticsReport diagnostics)
        {
            return "Diagnostics: " +
                   "assets=" + diagnostics.AssetNames.Count +
                   ", scenes=" + diagnostics.SceneNames.Count +
                   ", renderers=" + diagnostics.RendererCount +
                   ", materials=" + diagnostics.MaterialCount +
                   ", missing_shader_materials=" + diagnostics.MissingShaderMaterialCount +
                   ", pink_materials=" + diagnostics.PinkMaterialCount +
                   ", render_pipeline=" + FormatValue(diagnostics.RenderPipeline) +
                   ", shaders=" + FormatList(diagnostics.ShaderNames) +
                   ", warnings=" + FormatList(diagnostics.Warnings);
        }

        public static string FormatError(ObjectLoadError error)
        {
            return "Load error: " +
                   "code=" + error.Code +
                   ", http_status_code=" + (error.HttpStatusCode.HasValue ? error.HttpStatusCode.Value.ToString() : "none") +
                   ", message=" + FormatValue(error.Message);
        }

        private static string FormatList(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "none";
            }

            return string.Join("; ", values);
        }

        private static string FormatValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "none" : value;
        }
    }
}
