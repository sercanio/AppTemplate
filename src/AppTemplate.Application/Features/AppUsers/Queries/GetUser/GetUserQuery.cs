using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetUser;

public sealed record GetUserQuery(
   Guid UserId) : ICachedQuery<GetUserQueryResponse>
{
  public string CacheKey => $"users-{UserId}";

  public TimeSpan? Expiration => null;
}
