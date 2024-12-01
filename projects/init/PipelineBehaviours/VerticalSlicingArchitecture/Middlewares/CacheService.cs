using Microsoft.Extensions.Caching.Memory;

namespace VerticalSlicingArchitecture.Middlewares
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiration, CancellationToken cancellationToken = default);
        Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
    }


    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue(cacheKey, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiration, CancellationToken cancellationToken = default)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _cache.Set(cacheKey, value, cacheOptions);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }
    }

}
