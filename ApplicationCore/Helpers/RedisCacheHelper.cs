using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Web.Helpers
{
    public class RedisCacheHelper
    {
        private readonly IDistributedCache _distributedCache;

        public RedisCacheHelper(IDistributedCache distributedCache) 
        {
            _distributedCache = distributedCache;
        }

        public async Task<T> GetFromCacheAsync<T>(string cacheKey) where T : class
        {
            var cachedData = await _distributedCache.GetAsync(cacheKey);
            return ByteArrayToObj<T>(cachedData);
        }


        //將資料存進distributed cache
        public async Task SetCacheAsync<T>(string cacheKey, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpirationRelativeToNow)
        {
            var byteData = ObjectToByteArray(data);

            var cacheOptions = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            };

            await _distributedCache.SetAsync(cacheKey, byteData, cacheOptions);
        }


        //物件轉為distributed cache支援的Byte Array格式
        private static byte[] ObjectToByteArray(object obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        //distributed cache取得的ByteArray轉回物件
        private static T ByteArrayToObj<T>(byte[] byteArr) where T : class
        {
            return byteArr is null ? null : JsonSerializer.Deserialize<T>(byteArr);
        }
    }
}
