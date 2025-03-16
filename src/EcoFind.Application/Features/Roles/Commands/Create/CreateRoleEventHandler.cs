using EcoFind.Application.Repositories;
using EcoFind.Domain.Roles;
using EcoFind.Domain.Roles.DomainEvents;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;
using Myrtus.Clarity.Core.Application.Abstractions.Auditing;
using Myrtus.Clarity.Core.Application.Abstractions.Notification;
using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Application.Features.Roles.Commands.Create;

internal class CreateRoleEventHandler(
    IRolesRepository rolesRepository,
    IEmailSender emailSender,
    INotificationService notificationService,
    IAuditLogService auditLogService) : INotificationHandler<RoleCreatedDomainEvent>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task Handle(RoleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var role = await _rolesRepository.GetAsync(
            predicate: role => role.Id == notification.RoleId,
            cancellationToken: cancellationToken);

        AuditLog log = new()
        {
            User = role!.CreatedBy,
            Action = RoleDomainEvents.Created,
            Entity = role.GetType().Name,
            EntityId = role.Id.ToString(),
            Details = $"{role.GetType().Name} '{role.Name}' has been created."
        };
        await _auditLogService.LogAsync(log);

        await _notificationService.SendNotificationToUserGroupAsync(
            details: $"Role '{role.Name}' has been created by {role.CreatedBy}.",
            groupName: Role.Admin.Name.Value);

        //Mail mail = new(
        //    subject: "Role Created",
        //    textBody: $"Role '{role.Name}' has been created.",
        //    htmlBody: $"<p>Role '{role.Name}' has been created.</p>",
        //    toList:
        //    [
        //        new(encoding: Encoding.UTF8, name: role.CreatedBy, address: role.CreatedBy)
        //    ]);

        //await _emailService.SendEmailAsync(mail);
    }
}