using System;
using UnityEngine;

namespace Deucarian.ObjectLoading
{
    internal static class ObjectLoadTransportUtility
    {
        internal static bool TryParseCacheHash(string cacheHash, out Hash128 hash)
        {
            hash = default(Hash128);
            if (string.IsNullOrWhiteSpace(cacheHash))
            {
                return false;
            }

            try
            {
                hash = Hash128.Parse(cacheHash.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool IsSensitiveHeader(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            string normalized = name.Trim();
            return normalized.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                   || normalized.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase)
                   || normalized.Equals("X-Api-Key", StringComparison.OrdinalIgnoreCase)
                   || normalized.Equals("Api-Key", StringComparison.OrdinalIgnoreCase)
                   || normalized.IndexOf("token", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
