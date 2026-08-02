// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           GetWorkerWellBeingByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetWorkerWellBeingByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it
//                  belongs to a different tenant, so tenant membership is never revealed to an
//                  unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.WorkerWellBeing.Models;
using PlantSafety.Application.WorkerWellBeing.Queries;
using PlantSafety.Application.WorkerWellBeing.Repositories;

namespace PlantSafety.Application.WorkerWellBeing.Handlers;

/// <summary>Handles <see cref="GetWorkerWellBeingByIdQuery"/>.</summary>
public sealed class GetWorkerWellBeingByIdQueryHandler : IRequestHandler<GetWorkerWellBeingByIdQuery, Result<WorkerWellBeingItemDto>>
{
    private readonly IWorkerWellBeingRepository _repository;
    private readonly ILogger<GetWorkerWellBeingByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkerWellBeingByIdQueryHandler(IWorkerWellBeingRepository repository, ILogger<GetWorkerWellBeingByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkerWellBeingItemDto>> Handle(GetWorkerWellBeingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.WorkerWellBeingId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("WorkerWellBeing {WorkerWellBeingId} not found for Tenant {TenantId}", request.WorkerWellBeingId, request.TenantId);

                return Result<WorkerWellBeingItemDto>.Failure(new ApplicationError(
                    "WorkerWellBeing.WorkerWellBeing.NotFound",
                    "The requested worker wellbeing record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<WorkerWellBeingItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkerWellBeing {WorkerWellBeingId}", request.WorkerWellBeingId);

            return Result<WorkerWellBeingItemDto>.Failure(new ApplicationError(
                "WorkerWellBeing.WorkerWellBeing.RetrievalFailed",
                "Unable to retrieve worker wellbeing data at this time.",
                Guid.NewGuid()));
        }
    }
}
