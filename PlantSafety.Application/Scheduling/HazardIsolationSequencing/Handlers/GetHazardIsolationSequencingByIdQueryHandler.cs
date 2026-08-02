// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           GetHazardIsolationSequencingByIdQueryHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for GetHazardIsolationSequencingByIdQuery. Reads from the read-side
//                  repository (optimised stored proc; no domain-aggregate reconstruction). Returns
//                  a NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Models;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Queries;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Repositories;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Handlers;

/// <summary>Handles <see cref="GetHazardIsolationSequencingByIdQuery"/>.</summary>
public sealed class GetHazardIsolationSequencingByIdQueryHandler : IRequestHandler<GetHazardIsolationSequencingByIdQuery, Result<HazardIsolationSequencingItemDto>>
{
    private readonly IHazardIsolationSequencingRepository _repository;
    private readonly ILogger<GetHazardIsolationSequencingByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardIsolationSequencingByIdQueryHandler(IHazardIsolationSequencingRepository repository, ILogger<GetHazardIsolationSequencingByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardIsolationSequencingItemDto>> Handle(GetHazardIsolationSequencingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.HazardIsolationSequencingId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("HazardIsolationSequencing {HazardIsolationSequencingId} not found for Tenant {TenantId}", request.HazardIsolationSequencingId, request.TenantId);

                return Result<HazardIsolationSequencingItemDto>.Failure(new ApplicationError(
                    "Scheduling.HazardIsolationSequencing.NotFound",
                    "The requested hazard isolation sequencing record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<HazardIsolationSequencingItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardIsolationSequencing {HazardIsolationSequencingId}", request.HazardIsolationSequencingId);

            return Result<HazardIsolationSequencingItemDto>.Failure(new ApplicationError(
                "Scheduling.HazardIsolationSequencing.RetrievalFailed",
                "Unable to retrieve hazard isolation sequencing data at this time.",
                Guid.NewGuid()));
        }
    }
}
