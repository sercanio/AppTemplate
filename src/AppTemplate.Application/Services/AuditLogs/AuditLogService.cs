using System.Linq.Expressions;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AuditLogs;
using Microsoft.AspNetCore.SignalR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.SignalR.Hubs;
using Newtonsoft.Json;

namespace AppTemplate.Application.Services.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogsRepository _auditLogsRepository;
    private readonly IHubContext<AuditLogHub> _hubContext;
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(
        IAuditLogsRepository auditLogsRepository,
        IHubContext<AuditLogHub> hubContext,
        IUnitOfWork unitOfWork)
    {
        _auditLogsRepository = auditLogsRepository ?? throw new ArgumentNullException(nameof(auditLogsRepository));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        try
        {
            // Add the audit log using the repository
            await _auditLogsRepository.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();

            // Broadcast the log to all clients through SignalR
            string message = JsonConvert.SerializeObject(log);
            await _hubContext.Clients.All.SendAsync("ReceiveAuditLog", message);
        }
        catch (Exception ex)
        {
            // Log the error or handle it appropriately
            throw new Exception("Failed to save audit log", ex);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(
        string entity,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _auditLogsRepository.GetAuditLogsByEntityAsync(entity, entityId, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> SearchAuditLogsInJsonDataAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        return await _auditLogsRepository.SearchAuditLogsInJsonDataAsync(key, value, cancellationToken);
    }

    public async Task<IPaginatedList<AuditLog>> GetAllAsync(
        int pageIndex = 0,
        int pageSize = 10,
        bool includeSoftDeleted = false,
        Expression<Func<AuditLog, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<AuditLog, object>>[] include)
    {
        return await _auditLogsRepository.GetAllAsync(
            pageIndex,
            pageSize,
            includeSoftDeleted,
            predicate,
            cancellationToken,
            include);
    }
}
