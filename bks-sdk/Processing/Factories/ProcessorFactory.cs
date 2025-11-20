using bks.sdk.Processing.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace bks.sdk.Processing.Factories;

public class ProcessorFactory : IProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ProcessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBKSBusinessProcessor<TRequest, TResponse>? GetProcessor<TRequest, TResponse>()
        where TRequest : class
    {
        return _serviceProvider.GetService<IBKSTransactionProcessor<TRequest, TResponse>>();
    }
}
