using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.Roles.Commands.Create;

internal class CreateRoleEventHandler(
    IRolesRepository rolesRepository,
    IEmailSender emailSender,
    INotificationService notificationService,
    ILogger<CreateRoleCommandHander> logger) : INotificationHandler<RoleCreatedDomainEvent>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly INotificationService _notificationService = notificationService;

    public async Task Handle(RoleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling RoleCreatedDomainEvent for RoleId: {RoleId}", notification.RoleId);
    }
}