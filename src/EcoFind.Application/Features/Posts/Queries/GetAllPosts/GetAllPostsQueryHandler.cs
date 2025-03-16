using Ardalis.Result;
using EcoFind.Application.Repositories;
using EcoFind.Domain.Posts;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Data;

namespace EcoFind.Application.Features.Posts.Queries.GetAllPosts;

public sealed class GetAllPostsQueryHandler(IPostsRepository postRepository) : IRequestHandler<GetAllPostsQuery, Result<IPaginatedList<GetAllPostsQueryResponse>>>
{
    private readonly IPostsRepository _postRepository = postRepository;

    public async Task<Result<IPaginatedList<GetAllPostsQueryResponse>>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        IPaginatedList<Post> posts = await _postRepository.GetAllAsync(
            pageIndex: request.PageIndex,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        List<GetAllPostsQueryResponse> mappedPosts = posts.Items.Select(post =>
            new GetAllPostsQueryResponse(
                post.Id,
                post.Title,
                post.Content,
                post.Summary,
                post.Slug,
                post.Author,
                post.IsPublished,
                post.PublishedOnUtc)).ToList();

        PaginatedList<GetAllPostsQueryResponse> paginatedList = new(
            mappedPosts,
            posts.TotalCount,
            request.PageIndex,
            request.PageSize
        );

        return Result.Success<IPaginatedList<GetAllPostsQueryResponse>>(paginatedList);
    }
}
