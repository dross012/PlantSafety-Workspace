// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           ActivateObservationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateObservationCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
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

/// <summary>Handles <see cref="ActivateObservationCommand"/>.</summary>
public sealed class ActivateObservationCommandHandler : IRequestHandler<ActivateObservationCommand, Result<ObservationStatusResult>>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<ActivateObservationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateObservationCommandHandler(IObservationRepository repository, ILogger<ActivateObservationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ObservationStatusResult>> Handle(ActivateObservationCommand request, CancellationToken cancellationToken)
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

            var activatedBy = UserId.FromGuid(request.ActivatedByUserId);

            aggregate.Activate(
                activatedBy: activatedBy,
                activatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(activatedBy),
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
            _logger.LogError(ex, "Failed to activate Observation {ObservationId}", request.ObservationId);

            return Result<ObservationStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.Observation.ActivateFailed",
                "Unable to activate the observation at this time.",
                Guid.NewGuid()));
        }
    }
}
