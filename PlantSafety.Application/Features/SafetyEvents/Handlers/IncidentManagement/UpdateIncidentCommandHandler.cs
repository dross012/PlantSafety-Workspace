// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR handler for UpdateIncidentCommand.
//   By the time this handler executes: auth checked, inputs validated, transaction open, ETag checked by ConcurrencyBehavior.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="UpdateIncidentCommand"/>.
/// Updates mutable fields on an existing Incident and stages an outbox event.
/// </summary>
public sealed class UpdateIncidentCommandHandler : ICommandHandler<UpdateIncidentCommand, Guid>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly ILogger<UpdateIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public UpdateIncidentCommandHandler(
        IIncidentRepository incidentRepository,
        ILogger<UpdateIncidentCommandHandler> logger)
    {
        _incidentRepository = incidentRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(UpdateIncidentCommand request, CancellationToken cancellationToken)
    {
        await _incidentRepository.UpdateAsync(
            tenantId: request.TenantId,
            incidentId: request.IncidentId,
            title: request.Title,
            description: request.Description,
            severity: request.Severity,
            location: request.Location,
            assignedInvestigatorId: request.AssignedInvestigatorId,
            eTag: request.ETag!,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Incident {IncidentId} updated for Tenant {TenantId} by Actor {ActorId} CorrelationId {CorrelationId}",
            request.IncidentId, request.TenantId, request.ActorId, request.CorrelationId);

        return request.IncidentId;
    }
}
