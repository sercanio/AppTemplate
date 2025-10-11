using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandHandler(
    IAppUsersRepository userRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IUserContext userContext) : ICommandHandler<UpdateNotificationPreferencesCommand, UpdateNotificationPreferencesCommandResponse>
{
  private readonly IAppUsersRepository _userRepository = userRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IUserContext _userContext = userContext;

  public async Task<Result<UpdateNotificationPreferencesCommandResponse>> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetAsync(
        predicate: user => user.IdentityId == _userContext.IdentityId,
        include: query => query.
            Include(u => u.NotificationPreference),
        cancellationToken: cancellationToken);

    if (user == null)
    {
      return Result<UpdateNotificationPreferencesCommandResponse>.NotFound();
    }

    user.SetNotificationPreference(request.NotificationPreference);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await _cacheService.RemoveAsync($"users-{user.Id}", cancellationToken);

    return Result<UpdateNotificationPreferencesCommandResponse>.Success(
        new UpdateNotificationPreferencesCommandResponse(user.Id, user.NotificationPreference));
  }
}
