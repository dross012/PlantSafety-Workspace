// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           ArchiveDrillsAndExercisesCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveDrillsAndExercisesCommand. Mirrors
//                  ConductDrillsAndExercisesCommandHandler's conventions.
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

/// <summary>Handles <see cref="ArchiveDrillsAndExercisesCommand"/>.</summary>
public sealed class ArchiveDrillsAndExercisesCommandHandler : IRequestHandler<ArchiveDrillsAndExercisesCommand, Result<DrillsAndExercisesStatusResult>>
{
    private readonly IDrillsAndExercisesRepository _repository;
    private readonly ILogger<ArchiveDrillsAndExercisesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveDrillsAndExercisesCommandHandler(IDrillsAndExercisesRepository repository, ILogger<ArchiveDrillsAndExercisesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DrillsAndExercisesStatusResult>> Handle(ArchiveDrillsAndExercisesCommand request, CancellationToken cancellationToken)
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

            var archivedBy = UserId.FromGuid(request.ArchivedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(archivedBy),
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
            _logger.LogError(ex, "Failed to archive DrillsAndExercises drill {DrillsAndExercisesId}", request.DrillsAndExercisesId);

            return Result<DrillsAndExercisesStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.DrillsAndExercises.ArchiveFailed",
                "Unable to archive the drill at this time.",
                Guid.NewGuid()));
        }
    }
}
