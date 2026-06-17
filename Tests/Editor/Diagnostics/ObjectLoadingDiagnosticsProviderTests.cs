using System;
using Deucarian.Diagnostics;
using Deucarian.ObjectLoading.Diagnostics;
using NUnit.Framework;

namespace Deucarian.ObjectLoading.Diagnostics.Tests
{
    public sealed class ObjectLoadingDiagnosticsProviderTests
    {
        [Test]
        public void ProviderIncludesObjectLoadingFields()
        {
            ObjectLoadingDiagnosticSnapshot snapshot = new ObjectLoadingDiagnosticSnapshot
            {
                DisplayName = "Demo Object",
                CurrentPhase = ObjectLoadPhase.Completed,
                Progress = 1f,
                ElapsedMs = 120,
                SourceType = "DirectUrl",
                LatestLoadResult = ObjectLoadResult.Success("Loaded.", null),
                ActiveComponents = new ObjectLoadingComponentInfo
                {
                    SourceResolver = "DirectUrlSourceResolver",
                    SourceContentLoader = "SourceAssetBundleContentLoader",
                    Downloader = "UnityWebRequestObjectDownloader",
                    ContentLoader = "AssetBundleContentLoader",
                    Instantiator = "AssetBundleObjectInstantiator",
                    ObjectMetadataCollector = "DefaultObjectDiagnostics"
                },
                LatestTelemetry = new ObjectLoadTelemetry
                {
                    DownloadTimeMs = 10,
                    BundleLoadTimeMs = 20,
                    InstantiateTimeMs = 30,
                    TotalTimeMs = 60,
                    BytesReceived = 1024,
                    AssetCount = 2,
                    SceneCount = 1,
                    RendererCount = 4,
                    MaterialCount = 5,
                    MissingShaderMaterialCount = 0,
                    PinkMaterialCount = 0,
                    LoadStrategy = "asset-bundle",
                    CacheStatus = "hit"
                }
            };
            snapshot.LoadedObjects.Add(new ObjectLoadingLoadedObjectInfo
            {
                Name = "Loaded Root",
                ActiveSelf = true,
                Scene = "Sample"
            });

            DiagnosticReportBuilder builder = new DiagnosticReportBuilder();
            new ObjectLoadingDiagnosticProvider(new Source(snapshot)).Collect(builder);
            DiagnosticReport report = builder.Build();

            Assert.AreEqual(DiagnosticSeverity.Success, report.Severity);
            DiagnosticSection section = report.Sections[0];
            AssertItem(section, "object", "Demo Object");
            AssertItem(section, "phase", "Completed");
            AssertItem(section, "progress");
            AssertItem(section, "elapsed_ms", "120 ms");
            AssertItem(section, "download_time_ms", "10 ms");
            AssertItem(section, "bundle_load_time_ms", "20 ms");
            AssertItem(section, "instantiate_time_ms", "30 ms");
            AssertItem(section, "bytes_received", "1024");
            AssertItem(section, "asset_count", "2");
            AssertItem(section, "scene_count", "1");
            AssertItem(section, "renderer_count", "4");
            AssertItem(section, "material_count", "5");
            AssertItem(section, "missing_shader_material_count", "0");
            AssertItem(section, "pink_material_count", "0");
            AssertItem(section, "source", "DirectUrl");
            AssertItem(section, "strategy", "asset-bundle");
            AssertItem(section, "cache_status", "hit");
            AssertItem(section, "current_error", "none");
            AssertItem(section, "last_error", "none");
            AssertItem(section, "source_resolver", "DirectUrlSourceResolver");
            AssertItem(section, "source_content_loader", "SourceAssetBundleContentLoader");
            AssertItem(section, "downloader", "UnityWebRequestObjectDownloader");
            AssertItem(section, "content_loader", "AssetBundleContentLoader");
            AssertItem(section, "instantiator", "AssetBundleObjectInstantiator");
            AssertItem(section, "object_metadata_collector", "DefaultObjectDiagnostics");
            AssertItem(section, "loaded_object_1", "Loaded Root");
        }

        [Test]
        public void ProviderIncludesCurrentAndLastErrors()
        {
            ObjectLoadError currentError = ObjectLoadError.Create(
                ObjectLoadErrorCode.DownloadFailed,
                "Could not download bundle.");
            ObjectLoadError lastError = ObjectLoadError.Create(
                ObjectLoadErrorCode.ContentLoadFailed,
                "Could not load bundle.");

            ObjectLoadingDiagnosticSnapshot snapshot = new ObjectLoadingDiagnosticSnapshot
            {
                CurrentPhase = ObjectLoadPhase.Failed,
                CurrentError = currentError,
                LastError = lastError
            };

            DiagnosticReportBuilder builder = new DiagnosticReportBuilder();
            new ObjectLoadingDiagnosticProvider(new Source(snapshot)).Collect(builder);
            DiagnosticSection section = builder.Build().Sections[0];

            AssertItem(section, "current_error", "DownloadFailed");
            AssertItem(section, "last_error", "ContentLoadFailed");
        }

        [Test]
        public void RegisterIsExplicitAndIdempotentForSameSource()
        {
            DiagnosticProviderRegistry.Clear();
            Source source = new Source(new ObjectLoadingDiagnosticSnapshot());
            IDisposable first = null;
            IDisposable second = null;

            try
            {
                first = ObjectLoadingDiagnostics.Register(source);
                second = ObjectLoadingDiagnostics.Register(source);

                Assert.AreEqual(1, DiagnosticProviderRegistry.SnapshotProviders().Count);

                first.Dispose();
                first = null;

                Assert.AreEqual(1, DiagnosticProviderRegistry.SnapshotProviders().Count);

                second.Dispose();
                second = null;

                Assert.AreEqual(0, DiagnosticProviderRegistry.SnapshotProviders().Count);
            }
            finally
            {
                first?.Dispose();
                second?.Dispose();
                DiagnosticProviderRegistry.Clear();
            }
        }

        private static void AssertItem(DiagnosticSection section, string key, string value = null)
        {
            bool exists = section.Items.Exists(item =>
                item.Key == key && (value == null || item.Value == value));
            Assert.IsTrue(exists, "Expected diagnostic item '" + key + "'.");
        }

        private sealed class Source : IObjectLoadingDiagnosticsSource
        {
            private readonly ObjectLoadingDiagnosticSnapshot snapshot;

            public Source(ObjectLoadingDiagnosticSnapshot snapshot)
            {
                this.snapshot = snapshot;
            }

            public ObjectLoadingDiagnosticSnapshot CreateDiagnosticSnapshot()
            {
                return snapshot;
            }
        }
    }
}
