using System.Collections.Generic;
using NUnit.Framework;

namespace Deucarian.ObjectLoading.Tests
{
    public sealed class ObjectLoadStageResultTests
    {
        [Test]
        public void SourceResolveFactories_PreserveSourceOrError()
        {
            ObjectSource source = ObjectSource.LocalFile("C:/Bundles/model.bundle");
            ObjectLoadError error = ObjectLoadError.Create(
                ObjectLoadErrorCode.SourceResolutionFailed,
                "Source resolution failed.");

            ObjectSourceResolveResult success = ObjectSourceResolveResult.Success(source);
            ObjectSourceResolveResult failure = ObjectSourceResolveResult.Failure(error);

            Assert.True(success.Succeeded);
            Assert.AreSame(source, success.Source);
            Assert.IsNull(success.Error);
            Assert.False(failure.Succeeded);
            Assert.AreSame(error, failure.Error);
            Assert.IsNull(failure.Source);
        }

        [Test]
        public void DownloadFactories_PreserveTransportDataOrError()
        {
            byte[] bytes = { 1, 2, 3 };
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/octet-stream" }
            };
            ObjectLoadError error = ObjectLoadError.Create(
                ObjectLoadErrorCode.DownloadFailed,
                "Download failed.");

            ObjectDownloadResult success = ObjectDownloadResult.Success(bytes, 200, headers);
            ObjectDownloadResult successWithoutHeaders = ObjectDownloadResult.Success(bytes, 200, null);
            ObjectDownloadResult failure = ObjectDownloadResult.Failure(error);

            Assert.True(success.Succeeded);
            Assert.AreSame(bytes, success.Bytes);
            Assert.AreEqual(200, success.HttpStatusCode);
            Assert.AreSame(headers, success.ResponseHeaders);
            Assert.IsEmpty(successWithoutHeaders.ResponseHeaders);
            Assert.False(failure.Succeeded);
            Assert.AreSame(error, failure.Error);
        }

        [Test]
        public void ContentLoadFactories_PreserveContentTelemetryOrError()
        {
            AssetBundleContent content = new AssetBundleContent(null, null, null);
            ObjectLoadTelemetry telemetry = new ObjectLoadTelemetry { AssetCount = 3 };
            ObjectLoadError error = ObjectLoadError.Create(
                ObjectLoadErrorCode.ContentLoadFailed,
                "Content load failed.");

            ObjectContentLoadResult success = ObjectContentLoadResult.Success(content, telemetry);
            ObjectContentLoadResult successWithoutTelemetry = ObjectContentLoadResult.Success(content);
            ObjectContentLoadResult failure = ObjectContentLoadResult.Failure(error);

            Assert.True(success.Succeeded);
            Assert.AreSame(content, success.Content);
            Assert.AreSame(telemetry, success.Telemetry);
            Assert.NotNull(successWithoutTelemetry.Telemetry);
            Assert.False(failure.Succeeded);
            Assert.AreSame(error, failure.Error);
        }

        [Test]
        public void InstantiationFactories_PreserveHandleMessageOrError()
        {
            IObjectLoadHandle handle = new ObjectLoadHandle((UnityEngine.GameObject)null, null);
            ObjectLoadError error = ObjectLoadError.Create(
                ObjectLoadErrorCode.InstantiationFailed,
                "Instantiation failed.");

            ObjectInstantiationResult success = ObjectInstantiationResult.Success(handle, "Object instantiated.");
            ObjectInstantiationResult failure = ObjectInstantiationResult.Failure(error);

            Assert.True(success.Succeeded);
            Assert.AreSame(handle, success.Handle);
            Assert.AreEqual("Object instantiated.", success.Message);
            Assert.False(failure.Succeeded);
            Assert.AreSame(error, failure.Error);

            handle.Dispose();
        }
    }
}
