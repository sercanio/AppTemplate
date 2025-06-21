# copilot-instructions.md

## 🧠 Project Overview

This is a fullstack web application built using:

- **Frontend**: React 19, Tailwind CSS, shadcn/ui, Zustand, 
- **Backend**: ASP.NET Core Web API with Clean Architecture and Domain Driven Design principles
- **Database**: PostgreSQL
- **Caching**: Redis + ASP.NET MemoryCache
- **Logging**: Serilog
- **Containerization**: Docker
- **Testing**: xUnit, Moq, FluentAssertions
- **Authentication**: ASP.NET Identity + JWT
- **Authorization**: Custom claims transformation and policies
- **Realtime**: SignalR (for websocket-based features)

## 🗂️ Folder Structure

High-level folder layout for Backend:
.
├── AppTemplate.sln
├── LICENSE
├── core
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Authentication
│   │   ├── IJwtService.cs
│   │   ├── IUserContext.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Authentication.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Behaviours
│   │   ├── LoggingBehavior.cs
│   │   ├── Myrtus.Clarity.Core.Application.Abstractions.Behaviours.csproj
│   │   ├── QueryCachingBehavior.cs
│   │   └── ValidationBehavior.cs
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Caching
│   │   ├── ICacheService.cs
│   │   ├── ICachedQuery.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Caching.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Clock
│   │   ├── IDateTimeProvider.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Clock.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Data.Dapper
│   │   ├── ISqlConnectionFactory.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Data.Dapper.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Localization
│   │   ├── Myrtus.Clarity.Core.Application.Abstractions.Localization.csproj
│   │   └── Services
│   │       └── ILocalizationService.cs
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Mailing
│   │   ├── IMailService.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Mailing.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Messaging
│   │   ├── ICommand.cs
│   │   ├── ICommandHandler.cs
│   │   ├── IQuery.cs
│   │   ├── IQueryHandler.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Messaging.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Module
│   │   ├── IClarityModule.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Module.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Pagination
│   │   ├── IPaginatedList.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Pagination.csproj
│   ├── Myrtus.Clarity.Core.Application.Abstractions.Repositories
│   │   ├── IRepository.cs
│   │   └── Myrtus.Clarity.Core.Application.Abstractions.Repositories.csproj
│   ├── Myrtus.Clarity.Core.Application.Exceptions
│   │   ├── ConcurrencyException.cs
│   │   ├── ErrorHandlingService.cs
│   │   ├── Myrtus.Clarity.Core.Application.Exceptions.csproj
│   │   └── ValidationError.cs
│   ├── Myrtus.Clarity.Core.Domain.Abstractions
│   │   ├── DomainError.cs
│   │   ├── Entity.cs
│   │   ├── IAggregateRoot.cs
│   │   ├── IDomainEvent.cs
│   │   ├── IUnitOfWork.cs
│   │   ├── Mailing
│   │   │   ├── Mail.cs
│   │   │   ├── MailSettings.cs
│   │   │   └── ToEmail.cs
│   │   ├── Myrtus.Clarity.Core.Domain.Abstractions.csproj
│   │   └── ValueObject.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.Authentication.Azure
│   │   ├── AdminAuthorizationDelegatingHandler.cs
│   │   ├── AuthenticationOptions.cs
│   │   ├── AzureAdB2CJwtService.cs
│   │   ├── AzureAdB2COptions.cs
│   │   ├── ClaimsPrincipalExtensions.cs
│   │   ├── JwtBearerOptionsSetup.cs
│   │   ├── Models
│   │   │   ├── AuthorizationToken.cs
│   │   │   └── CredentialRepresentationModel.cs
│   │   ├── Myrtus.Clarity.Core.Infrastructure.Authentication.Azure.csproj
│   │   └── UserContext.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.Authorization
│   │   ├── HasPermissionAttribute.cs
│   │   ├── Myrtus.Clarity.Core.Infrastructure.Authorization.csproj
│   │   ├── PermissionAuthorizationPolicyProvider.cs
│   │   └── PermissionRequirement.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.Caching
│   │   ├── CacheOptions.cs
│   │   ├── CacheService.cs
│   │   └── Myrtus.Clarity.Core.Infrastructure.Caching.csproj
│   ├── Myrtus.Clarity.Core.Infrastructure.Clock
│   │   ├── DateTimeProvider.cs
│   │   └── Myrtus.Clarity.Core.Infrastructure.Clock.csproj
│   ├── Myrtus.Clarity.Core.Infrastructure.Data.Dapper
│   │   ├── DateOnlyTypeHandler.cs
│   │   ├── Myrtus.Clarity.Core.Infrastructure.Data.Dapper.csproj
│   │   └── SqlConnectionFactory.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.DynamicQuery
│   │   ├── DynamicQuery.cs
│   │   ├── Filter.cs
│   │   ├── IQueryableDynamicFilterExtensions.cs
│   │   ├── Myrtus.Clarity.Core.Infrastructure.DynamicQuery.csproj
│   │   └── Sort.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.Localization
│   │   ├── Myrtus.Clarity.Core.Infrastructure.Localization.csproj
│   │   └── Services
│   │       └── LocalizationService.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.Mailing.MailKit
│   │   ├── MailKitMailService.cs
│   │   └── Myrtus.Clarity.Core.Infrastructure.Mailing.MailKit.csproj
│   ├── Myrtus.Clarity.Core.Infrastructure.Outbox
│   │   ├── Myrtus.Clarity.Core.Infrastructure.Outbox.csproj
│   │   ├── OutboxMessage.cs
│   │   ├── OutboxOptions.cs
│   │   ├── ProcessOutboxMessagesJob.cs
│   │   └── ProcessOutboxMessagesJobSetup.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.Pagination
│   │   ├── Myrtus.Clarity.Core.Infrastructure.Pagination.csproj
│   │   └── PaginatedList.cs
│   ├── Myrtus.Clarity.Core.Infrastructure.SignalR
│   │   ├── Hubs
│   │   │   ├── AuditLogHub.cs
│   │   │   └── NotificationHub.cs
│   │   └── Myrtus.Clarity.Core.Infrastructure.SignalR.csproj
│   ├── Myrtus.Clarity.Core.WebApi
│   │   ├── BaseController.cs
│   │   ├── ErrorHandlingService.cs
│   │   ├── IErrorHandlingService.cs
│   │   └── Myrtus.Clarity.Core.WebApi.csproj
│   └── Myrtus.Clarity.Core.sln
├── docker-compose.yml
└── src
    ├── AppTemplate.Application
    │   ├── AppTemplate.Application.csproj
    │   ├── DependencyInjection.cs
    │   ├── Enums
    │   │   └── Operation.cs
    │   ├── Features
    │   │   ├── Accounts
    │   │   │   └── UpdateNotificationPreferences
    │   │   │       ├── UpdateNotificationPreferencesCommand.cs
    │   │   │       ├── UpdateNotificationPreferencesCommandHandler.cs
    │   │   │       └── UpdateNotificationPreferencesCommandResponse.cs
    │   │   ├── AppUsers
    │   │   │   ├── Commands
    │   │   │   │   └── Update
    │   │   │   │       └── UpdateUserRoles
    │   │   │   │           ├── AddUserRoleEventHandler.cs
    │   │   │   │           ├── RemoveUserRoleEventHandler.cs
    │   │   │   │           ├── UpdateUserRolesCommand.cs
    │   │   │   │           ├── UpdateUserRolesCommandHandler.cs
    │   │   │   │           └── UpdateUserRolesCommandResponse.cs
    │   │   │   └── Queries
    │   │   │       ├── GetAllUsers
    │   │   │       │   ├── GetAllUsersQuery.cs
    │   │   │       │   ├── GetAllUsersQueryHandler.cs
    │   │   │       │   └── GetAllysersQueryResponse.cs
    │   │   │       ├── GetAllUsersByRoleId
    │   │   │       │   ├── GetAllUsersByRoleIdQuery.cs
    │   │   │       │   ├── GetAllUsersByRoleIdQueryHandler.cs
    │   │   │       │   └── GetAllUsersByRoleIdQueryResponse.cs
    │   │   │       ├── GetAllUsersDynamic
    │   │   │       │   ├── GetAllUsersDynamicQuery.cs
    │   │   │       │   ├── GetAllUsersDynamicQueryHandler.cs
    │   │   │       │   └── GetAllUsersDynamicQueryResponse.cs
    │   │   │       ├── GetLoggedInUser
    │   │   │       │   ├── GetLoggedInUserQuery.cs
    │   │   │       │   ├── GetLoggedInUserQueryHandler.cs
    │   │   │       │   ├── LoggedInUserRolesDto.cs
    │   │   │       │   └── UserResponse.cs
    │   │   │       └── GetUser
    │   │   │           ├── GetUserQuery.cs
    │   │   │           ├── GetUserQueryHandler.cs
    │   │   │           └── GetUserQueryResponse.cs
    │   │   ├── Notifications
    │   │   │   ├── Commands
    │   │   │   │   └── MarkNotificationsAsRead
    │   │   │   │       ├── MarkNotificationsAsReadCommand.cs
    │   │   │   │       ├── MarkNotificationsAsReadCommandHandler.cs
    │   │   │   │       └── MarkNotificationsAsReadCommandResponse.cs
    │   │   │   └── Queries
    │   │   │       └── GetAllNotifications
    │   │   │           ├── GetAllNotificationsQuery.cs
    │   │   │           ├── GetAllNotificationsQueryHandler.cs
    │   │   │           ├── GetAllNotificationsQueryResponse.cs
    │   │   │           └── GetAllNotificationsWithUnreadCountResponse.cs
    │   │   ├── Permissions
    │   │   │   └── Queries
    │   │   │       └── GetAllPermissions
    │   │   │           ├── GetAllPermissionsQuery.cs
    │   │   │           ├── GetAllPermissionsQueryResponse.cs
    │   │   │           └── GetallPermissionsQueryHandler.cs
    │   │   ├── Roles
    │   │   │   ├── Commands
    │   │   │   │   ├── Create
    │   │   │   │   │   ├── CreateRoleCommand.cs
    │   │   │   │   │   ├── CreateRoleCommandHander.cs
    │   │   │   │   │   ├── CreateRoleCommandResponse.cs
    │   │   │   │   │   ├── CreateRoleEventHandler.cs
    │   │   │   │   │   └── CreateRoleValidationhandler.cs
    │   │   │   │   ├── Delete
    │   │   │   │   │   ├── DeleteRoleCommand.cs
    │   │   │   │   │   ├── DeleteRoleCommandHandler.cs
    │   │   │   │   │   ├── DeleteRoleCommandResponse.cs
    │   │   │   │   │   └── DeleteRoleEventHandler.cs
    │   │   │   │   └── Update
    │   │   │   │       ├── UpdatePermissions
    │   │   │   │       │   ├── AddRolePermissionEventHandler.cs
    │   │   │   │       │   ├── RemoveRolePermissionEventHandler.cs
    │   │   │   │       │   ├── UpdateRolePermissionsCommand.cs
    │   │   │   │       │   ├── UpdateRolePermissionsCommandHandler.cs
    │   │   │   │       │   ├── UpdateRolePermissionsCommandResponse.cs
    │   │   │   │       │   └── UpdateRolePermissionsDto.cs
    │   │   │   │       └── UpdateRoleName
    │   │   │   │           ├── UpdateRoleNameCommand.cs
    │   │   │   │           ├── UpdateRoleNameCommandHandler.cs
    │   │   │   │           ├── UpdateRoleNameCommandResponse.cs
    │   │   │   │           └── UpdateRoleNameEventHandler.cs
    │   │   │   └── Queries
    │   │   │       ├── GetAllRoles
    │   │   │       │   ├── GetAllRolesQuery.cs
    │   │   │       │   ├── GetAllRolesQueryHandler.cs
    │   │   │       │   └── GetAllRolesQueryResponse.cs
    │   │   │       └── GetRoleById
    │   │   │           ├── GetRoleByIdPermissionResponseDto.cs
    │   │   │           ├── GetRoleByIdQuery.cs
    │   │   │           ├── GetRoleByIdQueryHandler.cs
    │   │   │           ├── GetRoleByIdQueryResponse.cs
    │   │   │           └── GetRoleByIdQueryValidator.cs
    │   │   └── Statistics
    │   │       ├── Authentication
    │   │       │   └── Queries
    │   │       │       └── GetAuthenticationStatistics
    │   │       │           ├── GetAuthenticationStatisticsQuery.cs
    │   │       │           ├── GetAuthenticationStatisticsQueryHandler.cs
    │   │       │           └── GetAuthenticationStatisticsResponse.cs
    │   │       ├── Roles
    │   │       │   └── Queries
    │   │       │       └── GetRoleStatistics
    │   │       │           ├── GetRoleStatisticsQuery.cs
    │   │       │           ├── GetRoleStatisticsQueryHandler.cs
    │   │       │           └── GetRoleStatisticsResponse.cs
    │   │       └── Users
    │   │           └── Queries
    │   │               ├── GetUserRegistrationTrends
    │   │               │   ├── GetUserRegistrationTrendsQuery.cs
    │   │               │   ├── GetUserRegistrationTrendsQueryHandler.cs
    │   │               │   └── GetUserRegistrationTrendsResponse.cs
    │   │               └── GetUsersCount
    │   │                   ├── GetUsersCountQuery.cs
    │   │                   ├── GetUsersCountQueryHandler.cs
    │   │                   └── GetUsersCountQueryResponse.cs
    │   ├── Repositories
    │   │   ├── IAppUsersRepository.cs
    │   │   ├── INotificationsRepository.cs
    │   │   ├── IPermissionsRepository.cs
    │   │   └── IRolesRepository.cs
    │   └── Services
    │       ├── AppUsers
    │       │   ├── AppUsersService.cs
    │       │   └── IAppUsersService.cs
    │       ├── Authentication
    │       │   └── AuthenticationEventsService.cs
    │       ├── Notifications
    │       │   ├── INotificationService.cs
    │       │   └── NotificationService.cs
    │       ├── Roles
    │       │   ├── IRolesService.cs
    │       │   └── RolesService.cs
    │       └── Statistics
    │           ├── ActiveSessionService.cs
    │           └── IActiveSessionService.cs
    ├── AppTemplate.Domain
    │   ├── AppTemplate.Domain.csproj
    │   ├── AppUsers
    │   │   ├── AppUser.cs
    │   │   ├── AppUserErrors.cs
    │   │   ├── DomainEvents
    │   │   │   ├── AppUserCreatedDomainEvent.cs
    │   │   │   ├── AppUserDomainEvents.cs
    │   │   │   ├── AppUserRoleAddedDomainEvent.cs
    │   │   │   └── AppUserRoleRemovedDomainEvent.cs
    │   │   └── ValueObjects
    │   │       └── NotificationPreference.cs
    │   ├── Notifications
    │   │   └── Notification.cs
    │   └── Roles
    │       ├── DomainEvents
    │       │   ├── RoleCreatedDomainEvent.cs
    │       │   ├── RoleDeletedDomainEvent.cs
    │       │   ├── RoleDomainEvents.cs
    │       │   ├── RoleNameUpdatedDomainEvent.cs
    │       │   ├── RolePermissionAddedDomainEvent.cs
    │       │   └── RolePermissionRemovedDomainEvent.cs
    │       ├── Permission.cs
    │       ├── Role.cs
    │       ├── RoleError.cs
    │       ├── RolePermission.cs
    │       └── ValueObjects
    │           ├── RoleDefaultFlag.cs
    │           └── RoleName.cs
    ├── AppTemplate.Infrastructure
    │   ├── AppTemplate.Infrastructure.csproj
    │   ├── ApplicationDbContext.cs
    │   ├── Autorization
    │   │   ├── AuthorizationService.cs
    │   │   ├── CustomClaimsTransformation.cs
    │   │   ├── PermissionAuthorizationHandler.cs
    │   │   └── UserRolesResponse.cs
    │   ├── Configurations
    │   │   ├── AppUserConfiguration.cs
    │   │   ├── NotificationConfiguration.cs
    │   │   ├── OutboxMessageConfiguration.cs
    │   │   ├── PermissionConfiguration.cs
    │   │   └── RoleConfiguration.cs
    │   ├── DependencyInjection.cs
    │   ├── Migrations
    │   │   ├── 20250326215516_initial.Designer.cs
    │   │   ├── 20250326215516_initial.cs
    │   │   ├── 20250619051201_addStatisticsPermissions.Designer.cs
    │   │   ├── 20250619051201_addStatisticsPermissions.cs
    │   │   └── ApplicationDbContextModelSnapshot.cs
    │   └── Repositories
    │       ├── AppUsersRepository.cs
    │       ├── NotificationsRepository.cs
    │       ├── PermissionsRepository.cs
    │       ├── Repository.cs
    │       └── RolesRepository.cs
    ├── AppTemplate.Web
    │   ├── AppTemplate.Web.csproj
    │   ├── AppTemplate.Web.csproj.user
    │   ├── Areas
    │   │   └── Identity
    │   │       └── Pages
    │   │           ├── Account
    │   │           │   ├── AccessDenied.cshtml
    │   │           │   ├── AccessDenied.cshtml.cs
    │   │           │   ├── ConfirmEmail.cshtml
    │   │           │   ├── ConfirmEmail.cshtml.cs
    │   │           │   ├── ConfirmEmailChange.cshtml
    │   │           │   ├── ConfirmEmailChange.cshtml.cs
    │   │           │   ├── ExternalLogin.cshtml
    │   │           │   ├── ExternalLogin.cshtml.cs
    │   │           │   ├── ForgotPassword.cshtml
    │   │           │   ├── ForgotPassword.cshtml.cs
    │   │           │   ├── ForgotPasswordConfirmation.cshtml
    │   │           │   ├── ForgotPasswordConfirmation.cshtml.cs
    │   │           │   ├── Lockout.cshtml
    │   │           │   ├── Lockout.cshtml.cs
    │   │           │   ├── Login.cshtml
    │   │           │   ├── Login.cshtml.cs
    │   │           │   ├── LoginWith2fa.cshtml
    │   │           │   ├── LoginWith2fa.cshtml.cs
    │   │           │   ├── LoginWithRecoveryCode.cshtml
    │   │           │   ├── LoginWithRecoveryCode.cshtml.cs
    │   │           │   ├── Logout.cshtml
    │   │           │   ├── Logout.cshtml.cs
    │   │           │   ├── Manage
    │   │           │   │   ├── ChangePassword.cshtml
    │   │           │   │   ├── ChangePassword.cshtml.cs
    │   │           │   │   ├── DeletePersonalData.cshtml
    │   │           │   │   ├── DeletePersonalData.cshtml.cs
    │   │           │   │   ├── Disable2fa.cshtml
    │   │           │   │   ├── Disable2fa.cshtml.cs
    │   │           │   │   ├── DownloadPersonalData.cshtml
    │   │           │   │   ├── DownloadPersonalData.cshtml.cs
    │   │           │   │   ├── Email.cshtml
    │   │           │   │   ├── Email.cshtml.cs
    │   │           │   │   ├── EnableAuthenticator.cshtml
    │   │           │   │   ├── EnableAuthenticator.cshtml.cs
    │   │           │   │   ├── ExternalLogins.cshtml
    │   │           │   │   ├── ExternalLogins.cshtml.cs
    │   │           │   │   ├── GenerateRecoveryCodes.cshtml
    │   │           │   │   ├── GenerateRecoveryCodes.cshtml.cs
    │   │           │   │   ├── Index.cshtml
    │   │           │   │   ├── Index.cshtml.cs
    │   │           │   │   ├── ManageNavPages.cs
    │   │           │   │   ├── PersonalData.cshtml
    │   │           │   │   ├── PersonalData.cshtml.cs
    │   │           │   │   ├── ResetAuthenticator.cshtml
    │   │           │   │   ├── ResetAuthenticator.cshtml.cs
    │   │           │   │   ├── SetPassword.cshtml
    │   │           │   │   ├── SetPassword.cshtml.cs
    │   │           │   │   ├── ShowRecoveryCodes.cshtml
    │   │           │   │   ├── ShowRecoveryCodes.cshtml.cs
    │   │           │   │   ├── TwoFactorAuthentication.cshtml
    │   │           │   │   ├── TwoFactorAuthentication.cshtml.cs
    │   │           │   │   ├── _Layout.cshtml
    │   │           │   │   ├── _ManageNav.cshtml
    │   │           │   │   ├── _StatusMessage.cshtml
    │   │           │   │   └── _ViewImports.cshtml
    │   │           │   ├── Register.cshtml
    │   │           │   ├── Register.cshtml.cs
    │   │           │   ├── RegisterConfirmation.cshtml
    │   │           │   ├── RegisterConfirmation.cshtml.cs
    │   │           │   ├── ResendEmailConfirmation.cshtml
    │   │           │   ├── ResendEmailConfirmation.cshtml.cs
    │   │           │   ├── ResetPassword.cshtml
    │   │           │   ├── ResetPassword.cshtml.cs
    │   │           │   ├── ResetPasswordConfirmation.cshtml
    │   │           │   ├── ResetPasswordConfirmation.cshtml.cs
    │   │           │   ├── _StatusMessage.cshtml
    │   │           │   └── _ViewImports.cshtml
    │   │           ├── Error.cshtml
    │   │           ├── Error.cshtml.cs
    │   │           ├── _ValidationScriptsPartial.cshtml
    │   │           ├── _ViewImports.cshtml
    │   │           └── _ViewStart.cshtml
    │   ├── Attributes
    │   │   └── Permissions.cs
    │   ├── Controllers
    │   │   ├── Api
    │   │   │   ├── ApiVersions.cs
    │   │   │   └── v1.0
    │   │   │       ├── AccountsController.cs
    │   │   │       ├── NotificationsController.cs
    │   │   │       ├── PermissionsController.cs
    │   │   │       ├── RolesController.cs
    │   │   │       ├── SecurityController.cs
    │   │   │       ├── StatisticsController.cs
    │   │   │       └── UsersController.cs
    │   │   └── HomeController.cs
    │   ├── DependencyInjection.cs
    │   ├── Dockerfile
    │   ├── Extensions
    │   │   └── ApplicationBuilderExtension.cs
    │   ├── Logs
    │   ├── Middlewares
    │   │   ├── ExceptionHandlingMiddleware.cs
    │   │   ├── ForbiddenResponseMiddleware.cs
    │   │   ├── RateLimitExceededMiddleware.cs
    │   │   ├── RequestContextLoggingMiddleware.cs
    │   │   └── SessionTrackingMiddleware.cs
    │   ├── Models
    │   │   └── ErrorViewModel.cs
    │   ├── Program.cs
    │   ├── Properties
    │   │   ├── launchSettings.json
    │   │   ├── serviceDependencies.json
    │   │   └── serviceDependencies.local.json
    │   ├── Views
    │   │   ├── Home
    │   │   │   ├── Index.cshtml
    │   │   │   └── Privacy.cshtml
    │   │   ├── Shared
    │   │   │   ├── Error.cshtml
    │   │   │   ├── _FooterPartial.cshtml
    │   │   │   ├── _HeaderPartial.cshtml
    │   │   │   ├── _Layout.cshtml
    │   │   │   ├── _Layout.cshtml.css
    │   │   │   ├── _LoginPartial.cshtml
    │   │   │   ├── _ThemeTogglePartial.cshtml
    │   │   │   └── _ValidationScriptsPartial.cshtml
    │   │   ├── Users
    │   │   │   ├── Index.cshtml
    │   │   │   └── Index.cshtml.cs
    │   │   ├── _ViewImports.cshtml
    │   │   └── _ViewStart.cshtml
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   ├── package.json
    │   ├── services
    │   │   ├── AuthenticationEventsService.cs
    │   │   └── AzureEmailSender.cs
    │   └── wwwroot
    │       ├── css
    │       │   ├── bundle.css
    │       │   ├── main.min.css
    │       │   ├── main.min.css.map
    │       │   └── styles.css
    │       ├── favicon.ico
    │       ├── js
    │       │   ├── site.js
    │       │   └── theme
    │       │       └── theme-toggle.js
    │       └── sass
    │           └── main.scss
    ├── AppTemplate.sln
    ├── EcoFind.Infrastructure
    ├── WebUI
    └── src
        ├── AppTemplate.Infrastructure
        └── EcoFind.Infrastructure

127 directories, 349 files

High-level folder layout for Frontend:

.
├── Dockerfile
├── README.md
├── components.json
├── dist
│   ├── assets
│   │   ├── index-B3gVEHpS.css
│   │   └── index-uZ3cpQp0.js
│   ├── index.html
│   └── vite.svg
├── docker-entrypoint.sh
├── eslint.config.js
├── index.html
├── nginx.conf
├── package-lock.json
├── package.json
├── public
│   ├── index.html
│   └── vite.svg
├── src
│   ├── App.css
│   ├── App.tsx
│   ├── assets
│   │   └── react.svg
│   ├── auth
│   │   ├── services
│   │   │   └── authService.ts
│   │   └── store
│   │       └── authStore.ts
│   ├── components
│   │   ├── LoadingSplash.tsx
│   │   ├── RouteLoader.tsx
│   │   ├── SuspenseLoader.tsx
│   │   ├── TwoFactorLogin.tsx
│   │   ├── admin-footer.tsx
│   │   ├── admin-header.tsx
│   │   ├── admin-sidebar.tsx
│   │   ├── auth
│   │   │   ├── ProtectedRoute.tsx
│   │   │   └── PublicRoute.tsx
│   │   ├── theme-toggle.tsx
│   │   └── ui
│   │       ├── alert-dialog.tsx
│   │       ├── alert.tsx
│   │       ├── avatar.tsx
│   │       ├── badge.tsx
│   │       ├── button.tsx
│   │       ├── card.tsx
│   │       ├── checkbox.tsx
│   │       ├── dialog.tsx
│   │       ├── dropdown-menu.tsx
│   │       ├── input.tsx
│   │       ├── label.tsx
│   │       ├── select.tsx
│   │       ├── separator.tsx
│   │       ├── sheet.tsx
│   │       ├── sidebar.tsx
│   │       ├── skeleton.tsx
│   │       ├── sonner.tsx
│   │       ├── switch.tsx
│   │       ├── table.tsx
│   │       ├── tabs.tsx
│   │       ├── textarea.tsx
│   │       └── tooltip.tsx
│   ├── contexts
│   │   └── ThemeContext.tsx
│   ├── hooks
│   │   ├── use-mobile.ts
│   │   └── useRouteLoading.ts
│   ├── index.css
│   ├── layouts
│   │   └── RootLayout.tsx
│   ├── lib
│   │   ├── toast.ts
│   │   └── utils.ts
│   ├── main.tsx
│   ├── pages
│   │   ├── ErrorPage.tsx
│   │   ├── Login.tsx
│   │   ├── Profile.tsx
│   │   ├── Roles.tsx
│   │   ├── Settings.tsx
│   │   ├── Users.tsx
│   │   └── dashboard.tsx
│   ├── profile
│   │   ├── components
│   │   │   ├── AuthWarning.tsx
│   │   │   ├── DangerZone.tsx
│   │   │   ├── DeleteAccountModal.tsx
│   │   │   ├── EmailConfirmationAlert.tsx
│   │   │   ├── NotificationsTab.tsx
│   │   │   ├── ProfilePicture.tsx
│   │   │   ├── ProfileTab.tsx
│   │   │   ├── ProfileTabs.tsx
│   │   │   ├── SecurityTab.tsx
│   │   │   ├── SettingsTab.tsx
│   │   │   └── StatusMessages.tsx
│   │   ├── context
│   │   │   └── profileContext.tsx
│   │   ├── services
│   │   │   ├── accountService.ts
│   │   │   └── profilePictureService.ts
│   │   └── utils
│   │       └── imageUtils.ts
│   ├── roles
│   │   ├── components
│   │   │   └── RolesManagement.tsx
│   │   └── services
│   │       └── rolesManagementService.ts
│   ├── router
│   │   └── index.tsx
│   ├── routes
│   │   └── LazyRoutes.tsx
│   ├── services
│   │   └── statisticsService.ts
│   ├── styles
│   │   └── theme.css
│   ├── users
│   │   ├── components
│   │   │   └── UsersManagement.tsx
│   │   └── services
│   │       └── usersManagementService.ts
│   └── vite-env.d.ts
├── tsconfig.app.json
├── tsconfig.json
├── tsconfig.node.json
└── vite.config.ts

32 directories, 95 files


## 📐 Patterns & Conventions

- ASP.NET Core backend uses:
  - **CQRS** via MediatR (`/Features/{Feature}/Commands` and `/Features/{Feature}/Queries`)
  - **Dependency injection** everywhere (via constructor)
  - **Entity Framework Core** with unit of work pattern
  - **Primary Constructors** primary constructors as standard
- Frontend follows:
  - Component-based structure using shadcn/ui and Tailwind
  - Zustand for state management
  - API communication via REST and SignalR
- Logs are written using **Serilog** with structured JSON format.

## 📦 Important Components

- `useAuthStore()` – Zustand store for managing auth state in frontend
- `ICacheService` – Redis or MemoryCache wrapper for caching in backend
- `IRequestLogger` – Used in middleware to log API requests
- `AuditLogService` – Stores sensitive user action logs (use with care)
- `Permission-based access` – Controlled via `[HasPermission(Permissions.{PermissionName})]` attribute in backend

## 🚫 Copilot, Please Avoid:

- Auto-generating `DbContext` methods – use repositories and `IUnitOfWork`
- Suggesting `useEffect(() => fetch(...))` in frontend – use service layer
- Modifying `*.Designer.cs`, `Program.cs`, or `Startup.cs` directly
- Writing raw SQL – always use LINQ in backend

## ✅ Copilot, Please Do:
  **Frontend**
- Use `themedToast` for frontend notifications
- Suggest reusable UI components following Tailwind + shadcn/ui
- Keep frontend API calls clean, to keep clean components, 
  in components just call Zustand Store action or Context action,
  all try/catch logic should be handled in the Context/Store layer, 
  and use `themedToast` for error handling.

  **Backend**
- Help write MediatR handlers for `CreateXCommand`, `GetXQuery`, etc.
- Use `parseError()` utility for handling frontend errors
- Wrap backend service logic with try-catch and structured logging
- Use dependency injection when suggesting backend services
- Add cancellation tokens to all async backend methods
- Cache frequently used values (roles, permissions) with Redis
- On every code change suggest update copilot-instructions.md file but do not
  modify it directly

## 🧪 Examples

**Frontend Zustand Store Sample**
```ts
export const useAuthStore = create<AuthStoreState>((set) => ({
  user: null,
  setUser: (user) => set({ user }),
  logout: () => set({ user: null }),
}));

**Backend Caching Service Usage**

var cachedValue = await _cacheService.GetAsync<UserDto>(cacheKey);
if (cachedValue is not null) return cachedValue;

// Fallback to DB

**Frontend API Call Pattern**

try {
  const response = await api.post("/auth/login", credentials);
  useAuthStore.getState().setUser(response.data.user);
} catch (error) {
  themedToast("Login failed", "error");
}

