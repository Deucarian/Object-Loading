namespace Deucarian.ObjectLoading.Samples.SimpleUsage
{
    [ObjectKeySet]
    public static class ObjectKeys
    {
        public static ObjectKey Preview => new Definition();
        private sealed class Definition : ObjectKey
        {
            public Definition() : base("preview.model") { }
        }
    }
}
