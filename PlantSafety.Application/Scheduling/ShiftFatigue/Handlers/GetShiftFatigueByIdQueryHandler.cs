// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           GetShiftFatigueByIdQueryHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for GetShiftFatigueByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.ShiftFatigue.Models;
using PlantSafety.Application.Scheduling.ShiftFatigue.Queries;
using PlantSafety.Application.Scheduling.ShiftFatigue.Repositories;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Handlers;

/// <summary>Handles <see cref="GetShiftFatigueByIdQuery"/>.</summary>
public sealed class GetShiftFatigueByIdQueryHandler : IRequestHandler<GetShiftFatigueByIdQuery, Result<ShiftFatigueItemDto>>
{
    private readonly IShiftFatigueRepository _repository;
    private readonly ILogger<GetShiftFatigueByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetShiftFatigueByIdQueryHandler(IShiftFatigueRepository repository, ILogger<GetShiftFatigueByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ShiftFatigueItemDto>> Handle(GetShiftFatigueByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ShiftFatigueId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("ShiftFatigue {ShiftFatigueId} not found for Tenant {TenantId}", request.ShiftFatigueId, request.TenantId);

                return Result<ShiftFatigueItemDto>.Failure(new ApplicationError(
                    "Scheduling.ShiftFatigue.NotFound",
                    "The requested shift fatigue record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ShiftFatigueItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ShiftFatigue {ShiftFatigueId}", request.ShiftFatigueId);

            return Result<ShiftFatigueItemDto>.Failure(new ApplicationError(
                "Scheduling.ShiftFatigue.RetrievalFailed",
                "Unable to retrieve shift fatigue data at this time.",
                Guid.NewGuid()));
        }
    }
}
