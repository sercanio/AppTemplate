using Ardalis.Result;
using Myrtus.Clarity.Core.Application.Abstractions.Authentication;
using Myrtus.Clarity.Core.Application.Abstractions.Caching;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Domain.Abstractions;
using AppTemplate.Application.Repositories;

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
            include: user => user.NotificationPreference,
            cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result<UpdateNotificationPreferencesCommandResponse>.NotFound();
        }

        user.NotificationPreference.Update(
            request.NotificationPreference.IsInAppNotificationEnabled,
            request.NotificationPreference.IsEmailNotificationEnabled,
            request.NotificationPreference.IsPushNotificationEnabled);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"users-{user.Id}", cancellationToken);

        return Result<UpdateNotificationPreferencesCommandResponse>.Success(
            new UpdateNotificationPreferencesCommandResponse(user.Id, user.NotificationPreference));
    }
}
