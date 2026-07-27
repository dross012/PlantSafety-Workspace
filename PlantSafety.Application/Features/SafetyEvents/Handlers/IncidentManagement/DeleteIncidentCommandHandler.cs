// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR handler for DeleteIncidentCommand (soft-delete).
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Delete;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="DeleteIncidentCommand"/>.
/// Soft-deletes an Incident by setting its Status to <c>Archived</c>.
/// </summary>
public sealed class DeleteIncidentCommandHandler : ICommandHandler<DeleteIncidentCommand>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DeleteIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeleteIncidentCommandHandler(
        IIncidentRepository incidentRepository,
        ICacheService cacheService,
        ILogger<DeleteIncidentCommandHandler> logger)
    {
        _incidentRepository = incidentRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Handle(DeleteIncidentCommand request, CancellationToken cancellationToken)
    {
        await _incidentRepository.SoftDeleteAsync(
            tenantId: request.TenantId,
            incidentId: request.IncidentId,
            deletionReason: request.DeletionReason,
            eTag: request.ETag!,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        // See UpdateIncidentCommandHandler's remarks -- GetIncidentByIdQuery.CacheKey format.
        await _cacheService.RemoveAsync($"incidents:{request.TenantId}:{request.IncidentId}", cancellationToken);

        _logger.LogInformation(
            "Incident {IncidentId} soft-deleted for Tenant {TenantId} by Actor {ActorId} CorrelationId {CorrelationId}",
            request.IncidentId, request.TenantId, request.ActorId, request.CorrelationId);
    }
}
