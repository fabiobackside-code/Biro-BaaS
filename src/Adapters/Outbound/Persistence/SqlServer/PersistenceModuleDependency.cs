using Domain.Core.Ports.Outbound;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Adapters.Outbound.Persistence.SqlServer.Repositories;

namespace Adapters.Outbound.Persistence.SqlServer
{
    public static class PersistenceModuleDependency
    {
        public static IServiceCollection AddPersistenceAdapter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
