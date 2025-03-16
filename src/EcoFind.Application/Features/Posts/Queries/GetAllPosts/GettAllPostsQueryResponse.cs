namespace EcoFind.Application.Features.Posts.Queries.GetAllPosts;

public sealed record GetAllPostsQueryResponse(
    Guid Id, 
    string Title, 
    string Content,
    string Summary,
    string Slug,
    string Author,
    bool IsPublished,
    DateTime? PublishedDate);
