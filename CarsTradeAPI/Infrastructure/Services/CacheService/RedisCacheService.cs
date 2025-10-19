using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace CarsTradeAPI.Infrastructure.Services.CacheService
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache) 
        {
            _cache = cache;
        }


        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key)
        {
            string? cachedData = await _cache.GetStringAsync(key);
            if (cachedData == null) 
            {
                return default(T?);
            }

            return JsonSerializer.Deserialize<T>(cachedData);
        }


        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null)
        {
            string jsonData = JsonSerializer.Serialize(value);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();

            if (absoluteExpireTime.HasValue)
                options.AbsoluteExpirationRelativeToNow = absoluteExpireTime;

            if (slidingExpireTime.HasValue)
                options.SlidingExpiration = slidingExpireTime;

            await _cache.SetStringAsync(key, jsonData, options);
        }


        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

    }
}
