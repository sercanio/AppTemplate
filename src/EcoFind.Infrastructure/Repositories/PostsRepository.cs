using EcoFind.Application.Repositories;
using EcoFind.Domain.Posts;

namespace EcoFind.Infrastructure.Repositories;

internal sealed class PostsRepository(ApplicationDbContext dbContext) : Repository<Post>(dbContext), IPostsRepository
{
    public override async Task AddAsync(Post post)
    {
        await DbContext.AddAsync(post);
    }

    public override void Update(Post post)
    {
        DbContext.Update(post);
    }
}
