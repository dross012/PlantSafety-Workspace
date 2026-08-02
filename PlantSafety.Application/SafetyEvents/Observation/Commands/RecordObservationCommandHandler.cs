// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           RecordObservationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordObservationCommand. Pure orchestration: constructs the
//                  Observation aggregate via its domain factory method and persists it through
//                  the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.Observation.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.Observation.Commands;

/// <summary>Handles <see cref="RecordObservationCommand"/>.</summary>
public sealed class RecordObservationCommandHandler : IRequestHandler<RecordObservationCommand, Result<RecordObservationResult>>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<RecordObservationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordObservationCommandHandler(IObservationRepository repository, ILogger<RecordObservationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordObservationResult>> Handle(RecordObservationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var observerId = UserId.FromGuid(request.ObserverUserId);

            var aggregate = PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.Create(
                createdBy: observerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(observerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                observerId: observerId,
                observationType: request.ObservationType,
                hazardCategory: request.HazardCategory,
                location: request.Location,
                description: request.Description,
                correctiveActionRequired: request.CorrectiveActionRequired,
                followUpDueAtUtc: request.FollowUpDueAtUtc.HasValue ? UtcTimestamp.From(request.FollowUpDueAtUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RecordObservationResult>.Success(new RecordObservationResult(
                aggregate.ObservationId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record Observation for Tenant {TenantId}", request.TenantId);

            return Result<RecordObservationResult>.Failure(new ApplicationError(
                "SafetyEvents.Observation.RecordFailed",
                "Unable to record the observation at this time.",
                Guid.NewGuid()));
        }
    }
}
