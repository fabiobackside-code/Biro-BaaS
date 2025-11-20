using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Biro.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateBootstrapLogger();

        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder.AddService(serviceName))
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter();
            });

        return services;
    }
}
