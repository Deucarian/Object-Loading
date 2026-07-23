using System.Collections.Generic;

namespace Deucarian.ObjectLoading
{
    public sealed class ObjectSourceResolveResult
    {
        public bool Succeeded { get; private set; }
        public ObjectSource Source { get; private set; }
        public ObjectLoadError Error { get; private set; }

        public static ObjectSourceResolveResult Success(ObjectSource source)
        {
            return new ObjectSourceResolveResult
            {
                Succeeded = true,
                Source = source
            };
        }

        public static ObjectSourceResolveResult Failure(ObjectLoadError error)
        {
            return new ObjectSourceResolveResult
            {
                Succeeded = false,
                Error = error
            };
        }
    }

    public sealed class ObjectDownloadResult
    {
        public bool Succeeded { get; private set; }
        public byte[] Bytes { get; private set; }
        public long HttpStatusCode { get; private set; }
        public Dictionary<string, string> ResponseHeaders { get; private set; }
        public ObjectLoadError Error { get; private set; }

        public static ObjectDownloadResult Success(byte[] bytes,
                                                   long httpStatusCode,
                                                   Dictionary<string, string> responseHeaders)
        {
            return new ObjectDownloadResult
            {
                Succeeded = true,
                Bytes = bytes,
                HttpStatusCode = httpStatusCode,
                ResponseHeaders = responseHeaders ?? new Dictionary<string, string>()
            };
        }

        public static ObjectDownloadResult Failure(ObjectLoadError error)
        {
            return new ObjectDownloadResult
            {
                Succeeded = false,
                Error = error
            };
        }
    }

    public sealed class ObjectContentLoadResult
    {
        public bool Succeeded { get; private set; }
        public AssetBundleContent Content { get; private set; }
        public ObjectLoadTelemetry Telemetry { get; private set; }
        public ObjectLoadError Error { get; private set; }

        public static ObjectContentLoadResult Success(AssetBundleContent content,
                                                      ObjectLoadTelemetry telemetry = null)
        {
            return new ObjectContentLoadResult
            {
                Succeeded = true,
                Content = content,
                Telemetry = telemetry ?? ObjectLoadTelemetry.Empty()
            };
        }

        public static ObjectContentLoadResult Failure(ObjectLoadError error)
        {
            return new ObjectContentLoadResult
            {
                Succeeded = false,
                Error = error
            };
        }
    }

    public sealed class ObjectInstantiationResult
    {
        public bool Succeeded { get; private set; }
        public IObjectLoadHandle Handle { get; private set; }
        public string Message { get; private set; }
        public ObjectLoadError Error { get; private set; }

        public static ObjectInstantiationResult Success(IObjectLoadHandle handle, string message)
        {
            return new ObjectInstantiationResult
            {
                Succeeded = true,
                Handle = handle,
                Message = message
            };
        }

        public static ObjectInstantiationResult Failure(ObjectLoadError error)
        {
            return new ObjectInstantiationResult
            {
                Succeeded = false,
                Error = error
            };
        }
    }
}
