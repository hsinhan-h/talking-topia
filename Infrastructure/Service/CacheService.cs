using Microsoft.Extensions.Caching.Memory;

namespace Web.Services
{
    public class CacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        // 儲存資料到快取
        public void SetCache(string key, string value, int SlidingExpiration = 30)
        {
            var cacheExpirationOptions = new MemoryCacheEntryOptions
            {
                //AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(65535), // /*絕對過期時間*/
                SlidingExpiration = TimeSpan.FromMinutes(SlidingExpiration) // 滑動過期時間
            };

            _memoryCache.Set(key, value, cacheExpirationOptions);
        }

        // 讀取快取資料
        public string GetCache(string key)
        {
            if (_memoryCache.TryGetValue(key, out var value))
            {
                return value.ToString();
            }
            else
            {
                return string.Empty;
            };
        }

        // 清除快取資料
        public void RemoveCache(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
