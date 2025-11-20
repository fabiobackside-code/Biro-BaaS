using bks.sdk.Common.Enums;
using bks.sdk.Common.Results;
using bks.sdk.Core.Configuration;
using bks.sdk.Observability.Logging;
using bks.sdk.Processing.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace bks.sdk.Core.Pipeline.Steps;

public class ProcessingStep<TRequest, TResponse> : BasePipelineStep<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BKSFrameworkSettings _settings;
    private readonly IBKSLogger _logger;

    public override string StepName => "Processing";
    public override int Order => 3;

    public ProcessingStep(
        IServiceProvider serviceProvider,
        BKSFrameworkSettings settings,
        IBKSLogger logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
        _logger = logger;
    }

    public override async Task<Result<TResponse>> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        await OnStepStarting(request);

        try
        {
        _logger.Trace("Iniciando processamento via Transaction Processor");

        var result = await ProcessViaTransactionProcessorAsync(request, cancellationToken);

        await OnStepCompleted(request, result);
            return result;
        }
        catch (Exception ex)
        {
            await OnStepFailed(request, ex);
            throw;
        }
    }

    private async Task<Result<TResponse>> ProcessViaTransactionProcessorAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        var processor = _serviceProvider.GetService<IBKSTransactionProcessor<TRequest, TResponse>>();

        if (processor == null)
        {
            return Result<TResponse>.Failure($"Nenhum processador de transação encontrado para {typeof(TRequest).Name}");
        }

        return await processor.ProcessAsync(request, cancellationToken);
    }
}

