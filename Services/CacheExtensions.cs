using Microsoft.Extensions.Caching.Memory;

namespace WUIAM.Services
{
    /// <summary>
    /// Provides cached access to frequently-read data.
    /// Uses IMemoryCache with configurable TTLs per cache key.
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Default cache TTL for static data (roles, permissions, user types).
        /// </summary>
        private static readonly TimeSpan StaticCacheDuration = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Default cache TTL for semi-static data (departments, leave types, job categories).
        /// </summary>
        private static readonly TimeSpan SemiStaticCacheDuration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Default cache TTL for dynamic data (employee directories, leave balances).
        /// </summary>
        private static readonly TimeSpan DynamicCacheDuration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Get or add a cached value for static data (60-minute TTL).
        /// </summary>
        public static T? GetOrCreateStatic<T>(this IMemoryCache cache, string key, Func<T> factory) where T : class
        {
            return cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = StaticCacheDuration;
                entry.AbsoluteExpiration = DateTime.UtcNow + StaticCacheDuration;
                entry.Priority = CacheItemPriority.High;
                return factory();
            });
        }

        /// <summary>
        /// Get or add a cached value for semi-static data (30-minute TTL).
        /// </summary>
        public static T? GetOrCreateSemiStatic<T>(this IMemoryCache cache, string key, Func<T> factory) where T : class
        {
            return cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = SemiStaticCacheDuration;
                entry.AbsoluteExpiration = DateTime.UtcNow + SemiStaticCacheDuration;
                entry.Priority = CacheItemPriority.High;
                return factory();
            });
        }

        /// <summary>
        /// Get or add a cached value for dynamic data (10-minute TTL).
        /// </summary>
        public static T? GetOrCreateDynamic<T>(this IMemoryCache cache, string key, Func<T> factory) where T : class
        {
            return cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = DynamicCacheDuration;
                entry.AbsoluteExpiration = DateTime.UtcNow + DynamicCacheDuration;
                entry.Priority = CacheItemPriority.Normal;
                return factory();
            });
        }

        /// <summary>
        /// Invalidate a cached entry by key.
        /// </summary>
        public static void Invalidate(this IMemoryCache cache, string key)
        {
            cache.Remove(key);
        }

        /// <summary>
        /// Invalidate all entries matching a prefix.
        /// </summary>
        public static void InvalidatePrefix(this IMemoryCache cache, string prefix)
        {
            // IMemoryCache doesn't support prefix-based invalidation natively.
            // This is a placeholder for future implementation via a custom cache manager.
            // For now, callers should invalidate specific keys.
        }
    }
}
