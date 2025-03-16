using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;

namespace EcoFind.Application.Features.Posts.Queries.GetAllPosts
{
    public sealed record GetAllPostsQuery(int PageIndex, int PageSize) : IQuery<IPaginatedList<GetAllPostsQueryResponse>>;

}
