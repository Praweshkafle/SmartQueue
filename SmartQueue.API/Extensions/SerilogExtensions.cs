using Serilog;
using SmartQueue.API.Middleware;

namespace SmartQueue.API.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
    public static WebApplication UseSerilogAndCorrelation(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        });

        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}