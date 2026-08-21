using Microsoft.Extensions.Logging;
using SmartQueue.Domain.Interfaces;

namespace SmartQueue.Infrastructure.Services;

public class ResilientCacheService : ICacheService
{
    private readonly RedisCacheService _redis;
    private readonly ILogger<ResilientCacheService> _logger;

    public ResilientCacheService(
        RedisCacheService redis,
        ILogger<ResilientCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        try { return await _redis.GetAsync<T>(key, ct); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {Key}. Falling back to DB.", key);
            return default; 
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct)
    {
        try { await _redis.SetAsync(key, value, expiry, ct); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {Key}. Continuing without cache.", key);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        try { await _redis.DeleteAsync(key, ct); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DELETE failed for key {Key}. Continuing.", key);
        }
    }
}