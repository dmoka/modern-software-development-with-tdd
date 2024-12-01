using MediatR;

namespace VerticalSlicingArchitecture.Middlewares;

internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingPipelineBehavior(
        ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}