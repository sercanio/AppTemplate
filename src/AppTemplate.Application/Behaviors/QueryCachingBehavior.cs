using AppTemplate.Application.Services.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Behaviors;

public sealed class QueryCachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

    public QueryCachingBehavior(
        ICacheService cacheService,
        ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse? cachedResponse = await _cacheService.GetAsync<TResponse>(
            request.CacheKey,
            cancellationToken);

        string name = typeof(TRequest).Name;

        if (cachedResponse is not null)
        {
            _logger.LogInformation("Cache hit for {Query}", name);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for {Query}", name);

        TResponse response = await next();

        await _cacheService.SetAsync(request.CacheKey, response, request.Expiration, cancellationToken);

        return response;
    }
}
