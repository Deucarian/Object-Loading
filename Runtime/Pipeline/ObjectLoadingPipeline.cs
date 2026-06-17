using System;
using System.Collections;
using System.Diagnostics;

namespace Deucarian.ObjectLoading
{
    public sealed class ObjectLoadingPipeline : IObjectLoadingPipeline, IObjectLoadingDiagnosticsSource
    {
        private readonly IObjectSourceResolver _sourceResolver;
        private readonly IObjectSourceContentLoader _contentLoader;
        private readonly IObjectInstantiator _instantiator;
        private readonly IObjectDiagnostics _diagnostics;
        private readonly ObjectLoadingComponentInfo _componentInfo;
        private IObjectLoadHandle _lastHandle;
        private ObjectLoadRequestDebugSnapshot _currentRequest;
        private ObjectLoadProgress _currentProgress;
        private ObjectLoadResult _latestLoadResult;
        private ObjectLoadError _lastError;
        private ObjectLoadTelemetry _latestTelemetry;
        private ObjectDiagnosticsReport _latestObjectMetadata;
        private bool _isLoading;

        public ObjectLoadingPipeline()
            : this(new DirectUrlSourceResolver(),
                   new SourceAssetBundleContentLoader(),
                   new AssetBundleObjectInstantiator(),
                   new DefaultObjectDiagnostics())
        {
        }

        public ObjectLoadingPipeline(IObjectSourceResolver sourceResolver,
                                    IObjectSourceContentLoader contentLoader,
                                    IObjectInstantiator instantiator,
                                    IObjectDiagnostics diagnostics)
            : this(sourceResolver, contentLoader, instantiator, diagnostics, null, null)
        {
        }

        private ObjectLoadingPipeline(IObjectSourceResolver sourceResolver,
                                      IObjectSourceContentLoader contentLoader,
                                      IObjectInstantiator instantiator,
                                      IObjectDiagnostics diagnostics,
                                      IObjectDownloader downloader,
                                      IObjectContentLoader byteContentLoader)
        {
            _sourceResolver = sourceResolver ?? throw new ArgumentNullException(nameof(sourceResolver));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
            _diagnostics = diagnostics ?? new DefaultObjectDiagnostics();
            _componentInfo = CreateComponentInfo(
                _sourceResolver,
                _contentLoader,
                downloader,
                byteContentLoader,
                _instantiator,
                _diagnostics);
        }

        public ObjectLoadingPipeline(IObjectSourceResolver sourceResolver,
                                    IObjectDownloader downloader,
                                    IObjectContentLoader contentLoader,
                                    IObjectInstantiator instantiator,
                                    IObjectDiagnostics diagnostics)
            : this(sourceResolver,
                   new ByteArrayObjectSourceContentLoader(downloader, contentLoader),
                   instantiator,
                   diagnostics,
                   downloader,
                   contentLoader)
        {
        }

        public ObjectLoadResult LatestLoadResult
        {
            get { return _latestLoadResult; }
        }

        public ObjectLoadProgress CurrentProgress
        {
            get { return _currentProgress; }
        }

        public ObjectLoadError LastError
        {
            get { return _lastError; }
        }

        public ObjectLoadTelemetry LatestTelemetry
        {
            get { return _latestTelemetry; }
        }

        public ObjectDiagnosticsReport LatestObjectMetadata
        {
            get { return _latestObjectMetadata; }
        }

        public IObjectLoadHandle LastHandle
        {
            get { return _lastHandle; }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
        }

        public ObjectLoadingDiagnosticSnapshot CreateDiagnosticSnapshot()
        {
            ObjectLoadingDiagnosticSnapshot state = new ObjectLoadingDiagnosticSnapshot
            {
                IsLoading = _isLoading,
                CurrentRequest = _currentRequest,
                CurrentProgress = _currentProgress,
                LatestLoadResult = _latestLoadResult,
                LastError = _lastError,
                LatestTelemetry = _latestTelemetry,
                ObjectMetadata = _latestObjectMetadata,
                ActiveComponents = _componentInfo,
                DisplayName = _currentRequest?.DisplayName,
                SourceType = _currentRequest?.Source != null ? _currentRequest.Source.Type.ToString() : null,
                CurrentPhase = _currentProgress != null ? _currentProgress.Phase : ObjectLoadPhase.None,
                CurrentStage = _currentProgress != null ? _currentProgress.Stage : null,
                Progress = _currentProgress != null ? _currentProgress.Normalized : 0f,
                Message = _currentProgress != null ? _currentProgress.Message : _latestLoadResult?.Message,
                ElapsedMs = _currentProgress != null ? _currentProgress.ElapsedMs : _latestTelemetry?.TotalTimeMs ?? 0
            };

            AddLoadedObjectInfo(state, _lastHandle);
            return state;
        }

        public ObjectLoadingDiagnosticSnapshot CreateStateData()
        {
            return CreateDiagnosticSnapshot();
        }

        public IEnumerator LoadAsync(ObjectLoadRequest request, Action<ObjectLoadResult> onCompleted)
        {
            Stopwatch totalTimer = Stopwatch.StartNew();

            if (request == null)
            {
                ObjectLoadResult result = ObjectLoadResult.Failure(ObjectLoadError.Create(
                    ObjectLoadErrorCode.InvalidRequest,
                    "Object load request is missing."));
                _currentRequest = null;
                RecordProgress(ObjectLoadProgress.Create(ObjectLoadPhase.Failed, 1f, result.Message));
                RecordResult(result);
                onCompleted?.Invoke(result);
                yield break;
            }

            BeginLoad(request);
            Action<ObjectLoadProgress> originalProgress = request.Progress;
            request.Progress = progress =>
            {
                RecordProgress(progress);
                originalProgress?.Invoke(progress);
            };

            if (request.CancellationToken.IsCancellationRequested)
            {
                ObjectLoadResult result = ObjectLoadResult.Failure(ObjectLoadError.Create(
                    ObjectLoadErrorCode.Canceled,
                    "Object load was canceled before it started."));
                RecordProgress(ObjectLoadProgress.Create(ObjectLoadPhase.Failed, 1f, result.Message));
                RecordResult(result);
                RestoreProgress(request, originalProgress);
                onCompleted?.Invoke(result);
                yield break;
            }

            request.ReportProgress(ObjectLoadPhase.ResolvingSource, 0f, "Resolving object source.", 0, totalTimer.ElapsedMilliseconds);
            ObjectSourceResolveResult sourceResult = null;
            yield return _sourceResolver.ResolveAsync(request, value => sourceResult = value);
            if (sourceResult == null || !sourceResult.Succeeded)
            {
                totalTimer.Stop();
                ObjectLoadError error = sourceResult?.Error ?? ObjectLoadError.Create(
                    ObjectLoadErrorCode.SourceResolutionFailed,
                    "Could not resolve object source.");
                ReportFailed(request, totalTimer, null, error.Message);
                ObjectLoadResult result = ObjectLoadResult.Failure(error);
                RecordResult(result);
                RestoreProgress(request, originalProgress);
                onCompleted?.Invoke(result);
                yield break;
            }

            request.ReportProgress(ObjectLoadPhase.ResolvingSource, 1f, "Object source resolved.", 0, totalTimer.ElapsedMilliseconds);
            AssetBundleContent content = null;
            ObjectContentLoadResult contentResult = null;
            yield return _contentLoader.LoadAsync(sourceResult.Source, request, value => contentResult = value);
            if (contentResult == null || !contentResult.Succeeded)
            {
                totalTimer.Stop();
                ObjectLoadTelemetry contentFailureTelemetry = contentResult != null ? contentResult.Telemetry : null;
                ObjectLoadError error = contentResult?.Error ?? ObjectLoadError.Create(
                    ObjectLoadErrorCode.ContentLoadFailed,
                    "Could not load object content.");
                ReportFailed(request, totalTimer, contentFailureTelemetry, error.Message);
                ObjectLoadResult result = ObjectLoadResult.Failure(error, contentFailureTelemetry ?? _latestTelemetry);
                RecordResult(result);
                RestoreProgress(request, originalProgress);
                onCompleted?.Invoke(result);
                yield break;
            }

            content = contentResult.Content;
            ObjectInstantiationResult instantiationResult = null;
            Stopwatch instantiateTimer = Stopwatch.StartNew();
            yield return _instantiator.InstantiateAsync(content, request, value => instantiationResult = value);
            instantiateTimer.Stop();

            ObjectLoadTelemetry telemetry = contentResult.Telemetry ?? ObjectLoadTelemetry.Empty();
            telemetry.InstantiateTimeMs = instantiateTimer.ElapsedMilliseconds;
            telemetry.AssetCount = content != null && content.AssetNames != null ? content.AssetNames.Length : telemetry.AssetCount;
            telemetry.SceneCount = content != null && content.ScenePaths != null ? content.ScenePaths.Length : telemetry.SceneCount;
            totalTimer.Stop();
            telemetry.TotalTimeMs = totalTimer.ElapsedMilliseconds;

            if (instantiationResult == null || !instantiationResult.Succeeded)
            {
                content?.Unload(false);
                ObjectLoadError error = instantiationResult?.Error ?? ObjectLoadError.Create(
                    ObjectLoadErrorCode.InstantiationFailed,
                    "Could not instantiate object content.");
                ReportFailed(request, totalTimer, telemetry, error.Message);
                ObjectLoadResult result = ObjectLoadResult.Failure(error, telemetry);
                RecordResult(result);
                RestoreProgress(request, originalProgress);
                onCompleted?.Invoke(result);
                yield break;
            }

            _lastHandle = instantiationResult.Handle;
            request.ReportProgress(ObjectLoadPhase.Diagnostics, 0f, "Collecting object diagnostics.", telemetry.BytesReceived, totalTimer.ElapsedMilliseconds, telemetry);
            ObjectDiagnosticsReport report = _diagnostics.CreateReport(instantiationResult.Handle, content);
            CopyDiagnosticsToTelemetry(report, telemetry);
            request.ReportProgress(ObjectLoadPhase.Diagnostics, 1f, "Object diagnostics collected.", telemetry.BytesReceived, totalTimer.ElapsedMilliseconds, telemetry);
            request.ReportProgress(ObjectLoadPhase.Completed, 1f, instantiationResult.Message, telemetry.BytesReceived, totalTimer.ElapsedMilliseconds, telemetry);
            ObjectLoadResult success = ObjectLoadResult.Success(instantiationResult.Message, instantiationResult.Handle, report, telemetry);
            RecordResult(success);
            RestoreProgress(request, originalProgress);
            onCompleted?.Invoke(success);
        }

        public void UnloadLast()
        {
            if (_lastHandle == null)
            {
                return;
            }

            _lastHandle.Unload();
            _lastHandle = null;
        }

        private static ObjectLoadingComponentInfo CreateComponentInfo(IObjectSourceResolver sourceResolver,
                                                                      IObjectSourceContentLoader sourceContentLoader,
                                                                      IObjectDownloader downloader,
                                                                      IObjectContentLoader contentLoader,
                                                                      IObjectInstantiator instantiator,
                                                                      IObjectDiagnostics diagnostics)
        {
            return new ObjectLoadingComponentInfo
            {
                SourceResolver = GetTypeName(sourceResolver),
                SourceContentLoader = GetTypeName(sourceContentLoader),
                Downloader = GetTypeName(downloader),
                ContentLoader = GetTypeName(contentLoader),
                Instantiator = GetTypeName(instantiator),
                ObjectMetadataCollector = GetTypeName(diagnostics)
            };
        }

        private static string GetTypeName(object instance)
        {
            return instance == null ? null : instance.GetType().Name;
        }

        private void BeginLoad(ObjectLoadRequest request)
        {
            _isLoading = true;
            _currentRequest = request.CreateDebugSnapshot();
            _currentProgress = ObjectLoadProgress.Create(ObjectLoadPhase.ResolvingSource, 0f, "Starting object load.");
            _latestLoadResult = null;
            _lastError = null;
            _latestTelemetry = null;
            _latestObjectMetadata = null;
        }

        private void RecordProgress(ObjectLoadProgress progress)
        {
            if (progress == null)
            {
                return;
            }

            _currentProgress = progress;
            if (progress.Telemetry != null)
            {
                _latestTelemetry = progress.Telemetry;
            }
        }

        private void RecordResult(ObjectLoadResult result)
        {
            _latestLoadResult = result;
            _lastError = result?.Error;
            _latestTelemetry = result?.Telemetry ?? _latestTelemetry;
            _latestObjectMetadata = result?.Diagnostics ?? _latestObjectMetadata;
            _isLoading = false;
        }

        private static void RestoreProgress(ObjectLoadRequest request, Action<ObjectLoadProgress> originalProgress)
        {
            if (request != null)
            {
                request.Progress = originalProgress;
            }
        }

        private static void AddLoadedObjectInfo(ObjectLoadingDiagnosticSnapshot state, IObjectLoadHandle handle)
        {
            if (state == null || handle == null || handle.InstantiatedObjects == null)
            {
                return;
            }

            for (int i = 0; i < handle.InstantiatedObjects.Count; i++)
            {
                ObjectLoadingLoadedObjectInfo info = ObjectLoadingLoadedObjectInfo.FromGameObject(handle.InstantiatedObjects[i]);
                if (info != null)
                {
                    state.LoadedObjects.Add(info);
                }
            }
        }

        private sealed class ByteArrayObjectSourceContentLoader : IObjectSourceContentLoader
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

        private static void ReportFailed(ObjectLoadRequest request,
                                         Stopwatch totalTimer,
                                         ObjectLoadTelemetry telemetry,
                                         string message)
        {
            if (request == null)
            {
                return;
            }

            if (telemetry != null)
            {
                telemetry.TotalTimeMs = totalTimer.ElapsedMilliseconds;
            }

            request.ReportProgress(
                ObjectLoadPhase.Failed,
                1f,
                string.IsNullOrWhiteSpace(message) ? "Object loading failed." : message,
                telemetry != null ? telemetry.BytesReceived : 0,
                totalTimer.ElapsedMilliseconds,
                telemetry);
        }

        private static void CopyDiagnosticsToTelemetry(ObjectDiagnosticsReport report, ObjectLoadTelemetry telemetry)
        {
            if (report == null || telemetry == null)
            {
                return;
            }

            telemetry.RendererCount = report.RendererCount;
            telemetry.MaterialCount = report.MaterialCount;
            telemetry.MissingShaderMaterialCount = report.MissingShaderMaterialCount;
            telemetry.PinkMaterialCount = report.PinkMaterialCount;
        }
    }
}
