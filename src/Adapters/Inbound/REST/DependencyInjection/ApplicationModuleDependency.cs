using Application.UseCases;
using Domain.Core.Ports.Inbound;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Inbound.REST.DependencyInjection
{
    public static class ApplicationModuleDependency
    {
        public static IServiceCollection AddApplicationModule(this IServiceCollection services)
        {
            services.AddScoped<IAccountUseCases, AccountUseCases>();
            services.AddScoped<IClientUseCases, ClientUseCases>();
            services.AddScoped<ITransactionUseCases, TransactionUseCases>();
            return services;
        }
    }
}
