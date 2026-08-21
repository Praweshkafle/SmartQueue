namespace SmartQueue.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct);
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
}