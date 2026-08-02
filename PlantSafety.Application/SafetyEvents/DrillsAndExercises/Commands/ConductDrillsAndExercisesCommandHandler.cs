// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           ConductDrillsAndExercisesCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ConductDrillsAndExercisesCommand. GetByIdAsync is tenant-scoped,
//                  the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Interfaces;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Commands;

/// <summary>Handles <see cref="ConductDrillsAndExercisesCommand"/>.</summary>
public sealed class ConductDrillsAndExercisesCommandHandler : IRequestHandler<ConductDrillsAndExercisesCommand, Result<DrillsAndExercisesStatusResult>>
{
    private readonly IDrillsAndExercisesRepository _repository;
    private readonly ILogger<ConductDrillsAndExercisesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ConductDrillsAndExercisesCommandHandler(IDrillsAndExercisesRepository repository, ILogger<ConductDrillsAndExercisesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DrillsAndExercisesStatusResult>> Handle(ConductDrillsAndExercisesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DrillsAndExercisesId.FromGuid(request.DrillsAndExercisesId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<DrillsAndExercisesStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.DrillsAndExercises.NotFound",
                    "The requested drill could not be found.",
                    request.DrillsAndExercisesId,
                    ErrorSeverity.NotFound));
            }

            var conductedBy = UserId.FromGuid(request.ConductedByUserId);

            aggregate.Conduct(
                conductedBy: conductedBy,
                participantCount: request.ParticipantCount,
                conductedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(conductedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<DrillsAndExercisesStatusResult>.Success(new DrillsAndExercisesStatusResult(
                aggregate.DrillsAndExercisesId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to conduct DrillsAndExercises drill {DrillsAndExercisesId}", request.DrillsAndExercisesId);

            return Result<DrillsAndExercisesStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.DrillsAndExercises.ConductFailed",
                "Unable to record the drill as conducted at this time.",
                Guid.NewGuid()));
        }
    }
}
