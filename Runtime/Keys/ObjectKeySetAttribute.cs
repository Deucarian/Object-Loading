using System;

namespace Deucarian.ObjectLoading
{
    /// <summary>Marks an authoritative set of named ObjectKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ObjectKeySetAttribute : Attribute { }
}
