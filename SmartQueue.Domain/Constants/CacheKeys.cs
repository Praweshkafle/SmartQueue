namespace SmartQueue.Domain.Constants;

public static class CacheKeys
{
    public static string QueueStatus(Guid serviceId) => $"queue:status:{serviceId}";
}