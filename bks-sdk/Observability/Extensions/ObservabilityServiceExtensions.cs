using bks.sdk.Observability.Correlation;
using bks.sdk.Observability.Logging;
using bks.sdk.Observability.Performance;
using bks.sdk.Observability.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace bks.sdk.Observability.Extensions
{
    public static class ObservabilityServiceExtensions
    {
        public static IServiceCollection AddBKSFrameworkObservability(
            this IServiceCollection services)
        {
            services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            services.AddSingleton<IPerformanceTracker, PerformanceTracker>();
            services.AddSingleton<IBKSLogger, SerilogBKSLogger>();
            services.AddSingleton<IBKSTracer, OpenTelemetryBKSTracer>();

            return services;
        }
    }
}