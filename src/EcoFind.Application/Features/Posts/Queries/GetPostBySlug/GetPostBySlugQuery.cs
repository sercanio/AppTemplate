using Myrtus.Clarity.Core.Application.Abstractions.Caching;

namespace EcoFind.Application.Features.Posts.Queries.GetPostBySlug
{
    public sealed record GetPostBySlugQuery(string Slug) : ICachedQuery<GetPostBySlugQueryResponse>
    {
        public string CacheKey => $"posts-slug-{Slug}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}
