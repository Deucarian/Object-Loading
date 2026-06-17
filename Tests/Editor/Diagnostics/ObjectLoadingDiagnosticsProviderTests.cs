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
                    Downloader = "UnityWebRequestObjectDownloader",
                    ContentLoader = "AssetBundleContentLoader",
                    Instantiator = "AssetBundleObjectInstantiator"
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

            DiagnosticReportBuilder builder = new DiagnosticReportBuilder();
            new ObjectLoadingDiagnosticProvider(new Source(snapshot)).Collect(builder);
            DiagnosticReport report = builder.Build();

            Assert.AreEqual(DiagnosticSeverity.Success, report.Severity);
            Assert.IsTrue(report.Sections[0].Items.Exists(item => item.Key == "object" && item.Value == "Demo Object"));
            Assert.IsTrue(report.Sections[0].Items.Exists(item => item.Key == "download_time_ms" && item.Value == "10 ms"));
            Assert.IsTrue(report.Sections[0].Items.Exists(item => item.Key == "bytes_received" && item.Value == "1024"));
            Assert.IsTrue(report.Sections[0].Items.Exists(item => item.Key == "source_resolver" && item.Value == "DirectUrlSourceResolver"));
            Assert.IsTrue(report.Sections[0].Items.Exists(item => item.Key == "downloader" && item.Value == "UnityWebRequestObjectDownloader"));
            Assert.IsTrue(report.Sections[0].Items.Exists(item => item.Key == "strategy" && item.Value == "asset-bundle"));
            Assert.IsTrue(report.Sections[0].Items.Exists(item => item.Key == "cache_status" && item.Value == "hit"));
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
