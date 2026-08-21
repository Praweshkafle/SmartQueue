using System.Threading.RateLimiting;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using SmartQueue.API.Extensions;
using SmartQueue.API.Middleware;
using SmartQueue.Application.Auth.Commands.Register;
using SmartQueue.Application.Common;
using SmartQueue.Application.Tokens.Commands.IssueToken;
using SmartQueue.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddDatabase(builder.Configuration);
// builder.Services.AddDbContext<AppDbContext>(option =>
//     option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration
        .GetConnectionString("Redis");
});
// 1. Correct FluentValidation registration
builder.Services.AddValidatorsFromAssembly(typeof(RegisterCommand).Assembly);

// 2. Updated MediatR registration
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly);
    
    // FIX: Replace AddOpenBehavior with this line to force compilation mapping
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); 
});

builder.Services.AddRepository();
builder.Services.AddServices();

// Authentication
builder.Services.AddCustomAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("per-user", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromHours(1);
        opt.SegmentsPerWindow = 6;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddFixedWindowLimiter("per-ip", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            status = 429,
            message = "Too many requests, Please try again later.",
        }, ct);
    };
});

builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
        {
            options.UseNpgsqlConnection(
                builder.Configuration.GetConnectionString("DefaultConnection"));
        });
});
builder.Services.AddHangfireServer();
builder.Services.AddScoped<TokenExpiryJob>();    
builder.Services.AddScoped<QueueAutoAdvanceJob>(); 
builder.Services.AddHealthChecks(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocs();

var app = builder.Build();

app.MigrateDatabase();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        
        var errors = ex.Errors.Select(e => new 
        { 
            field = e.PropertyName, 
            error = e.ErrorMessage 
        });
        
        await context.Response.WriteAsJsonAsync(new
        {
            status = 400,
            message = "Validation failed.",
            errors
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync(new
        {
            status = 500,
            message = "An unexpected error occurred."
        });
    }
});

app.UseSerilogAndCorrelation();

app.UseSwaggerDocs();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangFireAuthFilter() }
});

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider
        .GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<TokenExpiryJob>(
        "token-expiry",
        job => job.ExecuteAsync(),
        Cron.MinuteInterval(5));

    recurringJobManager.AddOrUpdate<QueueAutoAdvanceJob>(
        "queue-auto-advance",
        job => job.ExecuteAsync(),
        Cron.MinuteInterval(3));
}

app.UseRateLimiter();
// add these before app.MapControllers()
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckExtensions.WriteResponse  // make this public
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckExtensions.WriteResponse
});
app.MapControllers();
app.Run();

public partial class Program { }