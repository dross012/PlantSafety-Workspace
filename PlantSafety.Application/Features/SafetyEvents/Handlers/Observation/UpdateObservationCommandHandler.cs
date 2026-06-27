// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for UpdateObservationCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Update;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.Observation;

/// <summary>Handles <see cref="UpdateObservationCommand"/>.</summary>
public sealed class UpdateObservationCommandHandler : ICommandHandler<UpdateObservationCommand, Guid>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<UpdateObservationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public UpdateObservationCommandHandler(IObservationRepository repository, ILogger<UpdateObservationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(UpdateObservationCommand request, CancellationToken cancellationToken)
    {
        await _repository.UpdateAsync(
            tenantId: request.TenantId,
            observationId: request.ObservationId,
            title: request.Title,
            description: request.Description,
            observationType: request.ObservationType,
            eTag: request.ETag!,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Observation {ObservationId} updated for Tenant {TenantId} by Actor {ActorId}",
            request.ObservationId, request.TenantId, request.ActorId);

        return request.ObservationId;
    }
}
