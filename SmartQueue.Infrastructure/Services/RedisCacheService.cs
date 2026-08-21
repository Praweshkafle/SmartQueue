using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Infrastructure.Services;

public class RedisCacheService:ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        var data = await _distributedCache.GetStringAsync(key, ct);
        if (data is null) return default;
        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };
        var data = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, data, options, ct);
    }

    public async Task DeleteAsync(string key, CancellationToken ct) =>
        await _distributedCache.RemoveAsync(key, ct);
}