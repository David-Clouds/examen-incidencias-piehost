using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace IncidenciasBicicletas.Web.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(data))
            {
                _logger.LogInformation("CACHE MISS - clave '{Key}' no encontrada en Redis, se consultará la base de datos", key);
                return default;
            }

            _logger.LogInformation("CACHE HIT - clave '{Key}' leída desde Redis", key);
            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiracion)
        {
            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiracion
            });
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
            _logger.LogInformation("CACHE INVALIDADA - clave '{Key}' eliminada de Redis", key);
        }
    }
}