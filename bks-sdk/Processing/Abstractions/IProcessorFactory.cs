namespace bks.sdk.Processing.Abstractions;

public interface IProcessorFactory
{
    IBKSBusinessProcessor<TRequest, TResponse>? GetProcessor<TRequest, TResponse>()
        where TRequest : class;
}