// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetProcessByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.ProcessManagement.Models.Processes;
using PlantSafety.Application.Features.ProcessManagement.Queries.Processes.GetProcess;

namespace PlantSafety.Application.Features.ProcessManagement.Handlers.Processes;

/// <summary>Handles <see cref="GetProcessByIdQuery"/>.</summary>
public sealed class GetProcessByIdQueryHandler : IQueryHandler<GetProcessByIdQuery, ProcessDefinitionDto?>
{
    private readonly IProcessRepository _repository;
    private readonly ILogger<GetProcessByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetProcessByIdQueryHandler(IProcessRepository repository, ILogger<GetProcessByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ProcessDefinitionDto?> Handle(GetProcessByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.ProcessId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Process {ProcessId} not found for Tenant {TenantId}", request.ProcessId, request.TenantId);
            return null;
        }

        return new ProcessDefinitionDto
        {
            ProcessId = model.ProcessId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            ProcessName = model.ProcessName,
            Category = model.Category,
            Status = model.Status,
            Version = model.Version,
            OwnerId = model.OwnerId,
            LastModifiedAtUtc = model.LastModifiedAtUtc,
            ETag = model.ETag,
        };
    }
}
