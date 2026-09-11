using System;
using System.Collections;
using System.Diagnostics;

namespace Deucarian.ObjectLoading
{
    internal sealed class ByteArrayObjectSourceContentLoader : IObjectSourceContentLoader
    {
        private readonly IObjectDownloader _downloader;
        private readonly IObjectContentLoader _contentLoader;

        public ByteArrayObjectSourceContentLoader(IObjectDownloader downloader, IObjectContentLoader contentLoader)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
        }

        public IEnumerator LoadAsync(ObjectSource source,
                                     ObjectLoadRequest request,
                                     Action<ObjectContentLoadResult> onCompleted)
        {
            if (source == null)
            {
                onCompleted?.Invoke(ObjectContentLoadResult.Failure(ObjectLoadError.Create(
                    ObjectLoadErrorCode.InvalidRequest,
                    "Object source is missing.")));
                yield break;
            }

            Stopwatch downloadTimer = Stopwatch.StartNew();
            ObjectLoadTelemetry telemetry = new ObjectLoadTelemetry
            {
                LoadStrategy = source.Type == ObjectSourceType.RawBytes ? "raw-bytes-legacy" : "byte-array-legacy",
                CacheMode = request != null ? request.CacheMode : ObjectLoadCacheMode.Default,
                CacheKey = request != null ? request.CacheKey : null,
                CacheHash = request != null ? request.CacheHash : null,
                CacheVersion = request != null ? request.CacheVersion : null,
                Crc = request != null ? request.Crc : 0,
                CacheStatus = "not-cacheable"
            };

            byte[] bytes = source.Bytes;
            if (source.Type != ObjectSourceType.RawBytes)
            {
                request?.ReportProgress(ObjectLoadPhase.Downloading, 0f, "Downloading AssetBundle bytes.", 0, 0, telemetry);
                ObjectDownloadResult downloadResult = null;
                yield return _downloader.DownloadAsync(source, request, value => downloadResult = value);
                downloadTimer.Stop();

                if (downloadResult == null || !downloadResult.Succeeded)
                {
                    ObjectLoadError error = downloadResult?.Error ?? ObjectLoadError.Create(
                        ObjectLoadErrorCode.DownloadFailed,
                        "Could not download object content.");
                    request?.ReportProgress(ObjectLoadPhase.Failed, 1f, error.Message, telemetry.BytesReceived, 0, telemetry);
                    onCompleted?.Invoke(ObjectContentLoadResult.Failure(error));
                    yield break;
                }

                bytes = downloadResult.Bytes;
                telemetry.DownloadTimeMs = downloadTimer.ElapsedMilliseconds;
                telemetry.BytesReceived = bytes != null ? bytes.Length : 0;
                request?.ReportProgress(ObjectLoadPhase.Downloading, 1f, "AssetBundle bytes downloaded.", telemetry.BytesReceived, 0, telemetry);
            }
            else
            {
                downloadTimer.Stop();
            }

            telemetry.BytesReceived = bytes != null ? bytes.Length : 0;

            Stopwatch bundleTimer = Stopwatch.StartNew();
            request?.ReportProgress(ObjectLoadPhase.LoadingBundle, 0f, "Loading AssetBundle from bytes.", telemetry.BytesReceived, 0, telemetry);
            ObjectContentLoadResult contentResult = null;
            yield return _contentLoader.LoadAsync(bytes, request, value => contentResult = value);
            bundleTimer.Stop();

            if (contentResult == null || !contentResult.Succeeded)
            {
                ObjectLoadError error = contentResult != null
                    ? contentResult.Error
                    : ObjectLoadError.Create(
                    ObjectLoadErrorCode.ContentLoadFailed,
                    "Could not load object content.");
                request?.ReportProgress(ObjectLoadPhase.Failed, 1f, error.Message, telemetry.BytesReceived, 0, telemetry);
                onCompleted?.Invoke(contentResult ?? ObjectContentLoadResult.Failure(error));
                yield break;
            }

            telemetry.BundleLoadTimeMs = bundleTimer.ElapsedMilliseconds;
            telemetry.AssetCount = contentResult.Content != null && contentResult.Content.AssetNames != null
                ? contentResult.Content.AssetNames.Length
                : 0;
            telemetry.SceneCount = contentResult.Content != null && contentResult.Content.ScenePaths != null
                ? contentResult.Content.ScenePaths.Length
                : 0;
            request?.ReportProgress(ObjectLoadPhase.DiscoveringContent, 1f, "AssetBundle content is ready.", telemetry.BytesReceived, 0, telemetry);

            onCompleted?.Invoke(ObjectContentLoadResult.Success(contentResult.Content, telemetry));
        }
    }
}
