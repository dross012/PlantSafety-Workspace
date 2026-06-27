// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateObservationCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.Observation;

/// <summary>Handles <see cref="CreateObservationCommand"/>. Returns the new ObservationId.</summary>
public sealed class CreateObservationCommandHandler : ICommandHandler<CreateObservationCommand, Guid>
{
    private readonly IObservationRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateObservationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateObservationCommandHandler(
        IObservationRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateObservationCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateObservationCommand request, CancellationToken cancellationToken)
    {
        var observationId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            observationId: observationId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            title: request.Title,
            description: request.Description,
            observationType: request.ObservationType,
            observedAtUtc: request.ObservedAtUtc,
            observedByWorkerId: request.ObservedByWorkerId,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Observation {ObservationId} created for Tenant {TenantId} by Actor {ActorId}",
            observationId, request.TenantId, request.ActorId);

        return observationId;
    }
}
