using NUnit.Framework;
using UnityEngine;

namespace Deucarian.ObjectLoading.Tests
{
    public sealed class ObjectLoadTransportUtilityTests
    {
        [TestCase("Authorization", true)]
        [TestCase(" Proxy-Authorization ", true)]
        [TestCase("X-Api-Key", true)]
        [TestCase("Api-Key", true)]
        [TestCase("X-Access-Token", true)]
        [TestCase("X-Trace", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsSensitiveHeader_ClassifiesCanonicalHeaderNames(string name, bool expected)
        {
            Assert.AreEqual(expected, ObjectLoadTransportUtility.IsSensitiveHeader(name));
        }

        [Test]
        public void TryParseCacheHash_ParsesTrimmedHash()
        {
            const string value = "0123456789abcdef0123456789abcdef";

            Hash128 hash;
            bool parsed = ObjectLoadTransportUtility.TryParseCacheHash("  " + value + "  ", out hash);

            Assert.True(parsed);
            Assert.AreEqual(Hash128.Parse(value), hash);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void TryParseCacheHash_RejectsMissingHash(string value)
        {
            Hash128 hash;
            bool parsed = ObjectLoadTransportUtility.TryParseCacheHash(value, out hash);

            Assert.False(parsed);
            Assert.AreEqual(default(Hash128), hash);
        }
    }
}
