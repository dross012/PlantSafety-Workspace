// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for UpdateNearMissCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Update;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;

/// <summary>Handles <see cref="UpdateNearMissCommand"/>.</summary>
public sealed class UpdateNearMissCommandHandler : ICommandHandler<UpdateNearMissCommand, Guid>
{
    private readonly INearMissRepository _repository;
    private readonly ILogger<UpdateNearMissCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public UpdateNearMissCommandHandler(INearMissRepository repository, ILogger<UpdateNearMissCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(UpdateNearMissCommand request, CancellationToken cancellationToken)
    {
        await _repository.UpdateAsync(
            tenantId: request.TenantId,
            nearMissId: request.NearMissId,
            title: request.Title,
            description: request.Description,
            potentialSeverity: request.PotentialSeverity,
            hazardCategory: request.HazardCategory,
            eTag: request.ETag!,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "NearMiss {NearMissId} updated for Tenant {TenantId} by Actor {ActorId}",
            request.NearMissId, request.TenantId, request.ActorId);

        return request.NearMissId;
    }
}
