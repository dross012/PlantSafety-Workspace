// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetWorkerWellBeingByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.WorkerWellBeing.Models.Records;
using PlantSafety.Application.Features.WorkerWellBeing.Queries.Records.GetRecord;

namespace PlantSafety.Application.Features.WorkerWellBeing.Handlers.Records;

/// <summary>Handles <see cref="GetWorkerWellBeingByIdQuery"/>.</summary>
public sealed class GetWorkerWellBeingByIdQueryHandler : IQueryHandler<GetWorkerWellBeingByIdQuery, WorkerWellBeingDto?>
{
    private readonly IWorkerWellBeingRecordRepository _repository;
    private readonly ILogger<GetWorkerWellBeingByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkerWellBeingByIdQueryHandler(IWorkerWellBeingRecordRepository repository, ILogger<GetWorkerWellBeingByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<WorkerWellBeingDto?> Handle(GetWorkerWellBeingByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.WorkerWellBeingId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("WorkerWellBeing {WorkerWellBeingId} not found for Tenant {TenantId}", request.WorkerWellBeingId, request.TenantId);
            return null;
        }

        return new WorkerWellBeingDto
        {
            WorkerWellBeingId = model.WorkerWellBeingId,
            TenantId = model.TenantId,
            WorkerId = model.WorkerId,
            WellBeingRiskLevel = model.WellBeingRiskLevel,
            Status = model.Status,
            RecordedAtUtc = model.RecordedAtUtc,
        };
    }
}
