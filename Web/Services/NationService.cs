using ApplicationCore.Entities;
using Web.Helpers;

namespace Web.Services
{
    public class NationService
    {
        private readonly IRepository _repository;
        private readonly RedisCacheHelper _cacheHelper;

        public NationService(IRepository repository, RedisCacheHelper cacheHelper)
        {
            _repository = repository;
            _cacheHelper = cacheHelper;
        }

        public async Task<List<Web.Entities.Nation>> GetNationsAsync()
        {
            var nations = await _repository.GetAll<Web.Entities.Nation>().ToListAsync();
            return nations;
        }

        public async Task<List<string>> GetNationNamesAsync()
        {
            //如果redis cache已有國籍清單資料, 直接return cache資料
            var cacheKey = "NationNames";
            var cachedData = await _cacheHelper.GetFromCacheAsync<List<string>>(cacheKey);
            if (cachedData != null)
                return cachedData;

            var nationNames = await _repository.GetAll<Web.Entities.Nation>()
                .Select(n => n.NationName)
                .ToListAsync();

            //將國籍清單查詢結果存到redis cache, SlidingExpiration設為30分, AbsoluteExpirationRelativeToNow設為1小時
            await _cacheHelper.SetCacheAsync(cacheKey, nationNames, TimeSpan.FromMinutes(30), TimeSpan.FromHours(1));

            return nationNames;
        }
    }
}
