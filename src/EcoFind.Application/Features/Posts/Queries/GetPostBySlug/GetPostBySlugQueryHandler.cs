using Ardalis.Result;
using EcoFind.Application.Repositories;
using EcoFind.Domain.Posts;
using Microsoft.Extensions.Caching.Memory;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace EcoFind.Application.Features.Posts.Queries.GetPostBySlug
{
    public sealed class GetPostBySlugQueryHandler : IQueryHandler<GetPostBySlugQuery, GetPostBySlugQueryResponse>
    {
        private readonly IPostsRepository _postRepository;
        private readonly IMemoryCache _cache;

        public GetPostBySlugQueryHandler(IPostsRepository postRepository, IMemoryCache cache)
        {
            _postRepository = postRepository;
            _cache = cache;
        }

        public async Task<Result<GetPostBySlugQueryResponse>> Handle(GetPostBySlugQuery request, CancellationToken cancellationToken)
        {
            // Check if the result is already in memory cache.
            if (_cache.TryGetValue(request.CacheKey, out GetPostBySlugQueryResponse cachedResponse))
            {
                return Result.Success(cachedResponse);
            }

            // Query the repository using the slug
            Post? post = await _postRepository.GetAsync(
                predicate: p => p.Slug == request.Slug,
                cancellationToken: cancellationToken);

            if (post is null)
            {
                return Result.NotFound("Post not found");
            }

            // Map the domain entity to the query response
            var response = new GetPostBySlugQueryResponse(
                post.Id,
                post.Title,
                post.Content,
                post.Summary,
                post.Slug,
                post.Author,
                post.PublishedOnUtc);

            // Create cache entry options using the expiration from the query
            var cacheEntryOptions = new MemoryCacheEntryOptions();
            if (request.Expiration.HasValue)
            {
                cacheEntryOptions.SetAbsoluteExpiration(request.Expiration.Value);
            }

            // Set the cache entry
            _cache.Set(request.CacheKey, response, cacheEntryOptions);

            return Result.Success(response);
        }
    }
}
