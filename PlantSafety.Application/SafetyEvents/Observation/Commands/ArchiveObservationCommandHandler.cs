// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           ArchiveObservationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveObservationCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ArchivePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.Observation.Interfaces;
using PlantSafety.Domain.SafetyEvents.Observation.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.Observation.Commands;

/// <summary>Handles <see cref="ArchiveObservationCommand"/>.</summary>
public sealed class ArchiveObservationCommandHandler : IRequestHandler<ArchiveObservationCommand, Result<ObservationStatusResult>>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<ArchiveObservationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveObservationCommandHandler(IObservationRepository repository, ILogger<ArchiveObservationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ObservationStatusResult>> Handle(ArchiveObservationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ObservationId.FromGuid(request.ObservationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ObservationStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.Observation.NotFound",
                    "The requested observation could not be found.",
                    request.ObservationId,
                    ErrorSeverity.NotFound));
            }

            var archivedBy = UserId.FromGuid(request.ArchivedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(archivedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ObservationStatusResult>.Success(new ObservationStatusResult(
                aggregate.ObservationId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive Observation {ObservationId}", request.ObservationId);

            return Result<ObservationStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.Observation.ArchiveFailed",
                "Unable to archive the observation at this time.",
                Guid.NewGuid()));
        }
    }
}
