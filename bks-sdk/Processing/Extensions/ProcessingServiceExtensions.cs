using bks.sdk.Core.Initialization;
using bks.sdk.Processing.Abstractions;
using bks.sdk.Processing.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace bks.sdk.Processing.Extensions;
public static class ProcessingServiceExtensions
{
    public static IServiceCollection AddBKSFrameworkProcessing(
        this IServiceCollection services,
        BKSFrameworkOptions options)
    {
        services.AddBKSFrameworkTransactionProcessor();

        return services;
    }

    public static IServiceCollection AddBKSFrameworkTransactionProcessor(this IServiceCollection services)
    {
        // Registrar processador de transações
        services.AddScoped(typeof(IBKSTransactionProcessor<,>), typeof(TransactionProcessor<,>));

        // Registrar processadores automaticamente
        RegisterTransactionProcessors(services);

        return services;
    }

    private static void RegisterTransactionProcessors(IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var processorTypes = assembly.GetTypes()
                .Where(t => t.BaseType != null &&
                           t.BaseType.IsGenericType &&
                           t.BaseType.GetGenericTypeDefinition() == typeof(Transactions.Processors.BaseTransactionProcessor<,>))
                .Where(t => !t.IsAbstract);

            foreach (var processorType in processorTypes)
            {
                // Registrar o processador específico
                services.AddScoped(processorType);

                // Registrar também pela interface genérica
                var baseType = processorType.BaseType!;
                var genericArgs = baseType.GetGenericArguments();
                var interfaceType = typeof(IBKSTransactionProcessor<,>).MakeGenericType(genericArgs);

                services.AddScoped(interfaceType, processorType);
            }
        }
    }
}

