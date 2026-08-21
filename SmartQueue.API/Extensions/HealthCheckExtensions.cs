namespace SmartQueue.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                config.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                tags: new[] { "ready", "db" })
            .AddRedis(
                config.GetConnectionString("Redis")!,
                name: "redis",
                tags: new[] { "ready", "cache" });

        return services;
    }

    public static Task WriteResponse(
        HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}