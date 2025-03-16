using EcoFind.Domain.Posts.DomainEvents;
using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Posts;

public class Post : Entity
{
    // rich domain model
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string Summary { get; private set; }
    public string Slug { get; private set; }
    public string Author { get; private set; }
    public DateTime PublishedOnUtc { get; private set; }
    public bool IsPublished { get; private set; }

    private Post(string title, string content, string summary, string slug, string author, DateTime publishedOnUtc, bool isPublished)
    {
        Title = title;
        Content = content;
        Summary = summary;
        Slug = slug;
        Author = author;
        PublishedOnUtc = publishedOnUtc;
        IsPublished = isPublished;
    }

    private Post()
    {
    }

    public static Post Create(string title, string content, string summary, string slug, string author, DateTime publishedOnUtc)
    {
        var post = new Post
        {
            Title = title,
            Content = content,
            Summary = summary,
            Slug = slug,
            Author = author,
            PublishedOnUtc = publishedOnUtc,
            IsPublished = false
        };
        post.RaiseDomainEvent(new PostCreatedDomainEvent(post));
        return post;
    }

    public void Publish()
    {
        IsPublished = true;
        RaiseDomainEvent(new PostPublishedDomainEvent(this));
    }

    public void Draft()
    {
        IsPublished = false;
        RaiseDomainEvent(new PostDraftedDomainEvent(this));
    }

    public void UpdateContent(string content)
    {
        Content = content;
        RaiseDomainEvent(new PostUpdatedDomainEvent(this));
    }

    public void UpdateSummary(string summary)
    {
        Summary = summary;
        RaiseDomainEvent(new PostUpdatedDomainEvent(this));
    }

    public void UpdateSlug(string slug)
    {
        Slug = slug;
        RaiseDomainEvent(new PostUpdatedDomainEvent(this));
    }

    public void UpdateTitle(string title)
    {
        Title = title;
        RaiseDomainEvent(new PostUpdatedDomainEvent(this));
    }

    public void UpdateAuthor(string author)
    {
        Author = author;
        RaiseDomainEvent(new PostUpdatedDomainEvent(this));
    }

    public void UpdatePublishedOnUtc(DateTime publishedOnUtc)
    {
        PublishedOnUtc = publishedOnUtc;
    }

    public void Delete()
    {
        MarkDeleted();
        RaiseDomainEvent(new PostDeletedDomainEvent(this));
    }
}
