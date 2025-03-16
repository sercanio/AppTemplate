namespace EcoFind.Application.Features.Posts.Queries.GetPostBySlug
{
    public sealed record GetPostBySlugQueryResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime? PublishedDate { get; set; }

        public GetPostBySlugQueryResponse() { }

        public GetPostBySlugQueryResponse(Guid id, string title, string content, string summary, string slug, string author, DateTime? publishedDate)
        {
            Id = id;
            Title = title;
            Content = content;
            Summary = summary;
            Slug = slug;
            Author = author;
            PublishedDate = publishedDate;
        }
    }
}
