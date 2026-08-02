// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           CompleteObservationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteObservationCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CompletePermitsCommandHandler.
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

/// <summary>Handles <see cref="CompleteObservationCommand"/>.</summary>
public sealed class CompleteObservationCommandHandler : IRequestHandler<CompleteObservationCommand, Result<ObservationStatusResult>>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<CompleteObservationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteObservationCommandHandler(IObservationRepository repository, ILogger<CompleteObservationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ObservationStatusResult>> Handle(CompleteObservationCommand request, CancellationToken cancellationToken)
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

            var completedBy = UserId.FromGuid(request.CompletedByUserId);

            aggregate.Complete(
                completedBy: completedBy,
                resolutionNotes: request.ResolutionNotes,
                completedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(completedBy),
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
            _logger.LogError(ex, "Failed to complete Observation {ObservationId}", request.ObservationId);

            return Result<ObservationStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.Observation.CompleteFailed",
                "Unable to complete the observation at this time.",
                Guid.NewGuid()));
        }
    }
}
