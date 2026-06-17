using Microsoft.Extensions.Caching.Memory;

namespace WUIAM.Services
{
    /// <summary>
    /// Service for managing cached data with configurable expiration policies.
    /// </summary>
    public interface ICachingService
    {
        /// <summary>
        /// Gets a cached value by key.
        /// </summary>
        T? Get<T>(string key);

        /// <summary>
        /// Sets a cached value with absolute expiration.
        /// </summary>
        void Set(string key, object value, TimeSpan expiration);

        /// <summary>
        /// Sets a cached value with sliding expiration.
        /// </summary>
        void SetSliding(string key, object value, TimeSpan slidingExpiration);

        /// <summary>
        /// Removes a cached value by key.
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Checks if a key exists in the cache.
        /// </summary>
        bool Contains(string key);

        /// <summary>
        /// Clears all cached values.
        /// </summary>
        void ClearAll();
    }

    /// <summary>
    /// Implementation of the caching service using MemoryCache.
    /// </summary>
    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _cache;

        public CachingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T? Get<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }

        public void Set(string key, object value, TimeSpan expiration)
        {
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.Normal
            });
        }

        public void SetSliding(string key, object value, TimeSpan slidingExpiration)
        {
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                Priority = CacheItemPriority.Normal
            });
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public bool Contains(string key)
        {
            return _cache.TryGetValue(key, out _);
        }

        public void ClearAll()
        {
            // MemoryCache doesn't support clearing all entries directly
            // We need to track keys manually or use a cache region
            // For now, this is a no-op placeholder
        }
    }
}
