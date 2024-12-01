using MediatR;
using VerticalSlicingArchitecture.Shared;

namespace VerticalSlicingArchitecture.Middlewares;

public interface IQuery<TResponse> : IRequest<TResponse>;

public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery
{
}

public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}

internal sealed class QueryCachingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
    where TResponse : Result
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<QueryCachingPipelineBehavior<TRequest, TResponse>> _logger;

    public QueryCachingPipelineBehavior(
        ICacheService cacheService,
        ILogger<QueryCachingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Try to get the cached result
        TResponse? cachedResult = await _cacheService.GetAsync<TResponse>(
            request.CacheKey,
            cancellationToken);

        string requestName = typeof(TRequest).Name;

        if (cachedResult is not null)
        {
            _logger.LogInformation("Cache hit for {RequestName}", requestName);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for {RequestName}", requestName);

        // Proceed to the next handler
        TResponse result = await next();

        // If the result is successful, cache it
        if (result.IsSuccess)
        {
            await _cacheService.SetAsync(
                request.CacheKey,
                result,
                request.Expiration,
                cancellationToken);
        }

        return result;
    }
}