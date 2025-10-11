namespace AppTemplate.Application.Services.Authentication;

public interface IUserContext
{
  Guid UserId { get; }

  string IdentityId { get; set; }
}