using Deucarian.Logging;

namespace Deucarian.ObjectLoading
{
    /// <summary>
    /// Package-level log categories for Object Loading.
    /// </summary>
    public static class ObjectLoadingLog
    {
        public static readonly DLog General = DLog.For("ObjectLoading");
        public static readonly DLog Downloader = DLog.For("ObjectLoading.Downloader");
        public static readonly DLog Loader = DLog.For("ObjectLoading.Loader");
        public static readonly DLog Instantiation = DLog.For("ObjectLoading.Instantiation");
        public static readonly DLog Diagnostics = DLog.For("ObjectLoading.Diagnostics");
    }
}
