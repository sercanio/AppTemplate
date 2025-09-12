namespace AppTemplate.Application.Authentication;

public interface IUserContext
{
  Guid UserId { get; }

  string IdentityId { get; set; }
}