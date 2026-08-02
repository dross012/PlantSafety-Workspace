// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           GetDrillsAndExercisesByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetDrillsAndExercisesByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  tenantId is pushed into the repository call itself
//                  (usp_DrillsAndExercises_GetById's WHERE clause), not just checked here after
//                  the fact -- the post-fetch TenantId comparison below is defense-in-depth, not
//                  the primary control. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Queries;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Handlers;

/// <summary>Handles <see cref="GetDrillsAndExercisesByIdQuery"/>.</summary>
public sealed class GetDrillsAndExercisesByIdQueryHandler : IRequestHandler<GetDrillsAndExercisesByIdQuery, Result<DrillsAndExercisesDetailDto>>
{
    private readonly IDrillsAndExercisesRepository _repository;
    private readonly ILogger<GetDrillsAndExercisesByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDrillsAndExercisesByIdQueryHandler(IDrillsAndExercisesRepository repository, ILogger<GetDrillsAndExercisesByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DrillsAndExercisesDetailDto>> Handle(GetDrillsAndExercisesByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.DrillsAndExercisesId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("DrillsAndExercises {DrillsAndExercisesId} not found for Tenant {TenantId}", request.DrillsAndExercisesId, request.TenantId);

                return Result<DrillsAndExercisesDetailDto>.Failure(new ApplicationError(
                    "SafetyEvents.DrillsAndExercises.NotFound",
                    "The requested drill could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<DrillsAndExercisesDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve DrillsAndExercises {DrillsAndExercisesId}", request.DrillsAndExercisesId);

            return Result<DrillsAndExercisesDetailDto>.Failure(new ApplicationError(
                "SafetyEvents.DrillsAndExercises.RetrievalFailed",
                "Unable to retrieve drill data at this time.",
                Guid.NewGuid()));
        }
    }
}
