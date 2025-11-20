using bks.sdk.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;

namespace bks.sdk.Middlewares.Observability
{
    public class ObservabilityMiddleware
    {
        private readonly RequestDelegate _next;

        public ObservabilityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            var settings = new BKSFrameworkSettings();
            configuration.GetSection("BKSFramework").Bind(settings);

            ConfigureSerilog(settings);
            ConfigureOpenTelemetry(context, settings);

            await _next(context);
        }

        private void ConfigureSerilog(BKSFrameworkSettings settings)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(GetSerilogLevel(settings.Observability.Logging.Level))
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", settings.ApplicationName)
                .Enrich.WithProperty("ServiceName", settings.Observability.ServiceName)
                .Enrich.WithProperty("ServiceVersion", settings.Observability.ServiceVersion)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("ProcessId", Environment.ProcessId);

            if (settings.Observability.Logging.WriteToConsole)
            {
                loggerConfig = loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            if (settings.Observability.Logging.WriteToFile)
            {
                var filePath = settings.Observability.Logging.FilePath.Replace("{ApplicationName}", settings.ApplicationName);
                loggerConfig = loggerConfig.WriteTo.File(
                    path: filePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            Log.Logger = loggerConfig.CreateLogger();
        }

        private void ConfigureOpenTelemetry(HttpContext context, BKSFrameworkSettings settings)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: settings.Observability.ServiceName,
                    serviceVersion: settings.Observability.ServiceVersion);

            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("bks.sdk")
                .Build();
        }

        private LogEventLevel GetSerilogLevel(string level)
        {
            return level.ToLowerInvariant() switch
            {
                "trace" or "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" or "info" => LogEventLevel.Information,
                "warning" or "warn" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
}