using MediatR;
using Serilog.Context;

namespace VerticalSlicingArchitecture.Middlewares;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    private readonly ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public RequestLoggingPipelineBehavior(
        ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ///Self exercise:
        ///1. Log request processing including the request name
        ///2a. If successful, log completed status
        ///2b. If failure, log that request is completed with error

        TResponse result = await next();

        return result;
    }
}