// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: MediatR handler for AddIncidentNoteCommand.
//   Loads the current Incident, rehydrates the Domain aggregate, applies AddNote, persists via
//   IIncidentRepository.AddNoteAsync (writes only the pending timeline entry; the Incident row
//   itself is untouched since notes have no dedicated column).
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.AddNote;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>Handles <see cref="AddIncidentNoteCommand"/>.</summary>
public sealed class AddIncidentNoteCommandHandler : ICommandHandler<AddIncidentNoteCommand, Guid>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly ILogger<AddIncidentNoteCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AddIncidentNoteCommandHandler(
        IIncidentRepository incidentRepository,
        IIncidentReadRepository incidentReadRepository,
        ILogger<AddIncidentNoteCommandHandler> logger)
    {
        _incidentRepository = incidentRepository;
        _incidentReadRepository = incidentReadRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(AddIncidentNoteCommand request, CancellationToken cancellationToken)
    {
        var correlationId = CorrelationId.FromGuid(request.CorrelationId);
        var causationId = CausationId.FromGuid(request.CausationId ?? request.CorrelationId);
        var incidentManagementId = IncidentManagementId.FromGuid(request.IncidentId);

        var currentRow = await _incidentReadRepository.GetByIdAsync(
            tenantId: request.TenantId,
            incidentId: request.IncidentId,
            cancellationToken: cancellationToken);

        if (currentRow is null)
            throw new IncidentManagementNotFoundException(incidentManagementId, correlationId, causationId);

        var aggregate = IncidentReadModelRehydrator.Rehydrate(incidentManagementId, currentRow);

        aggregate.AddNote(
            text: request.Text,
            nowUtc: UtcTimestamp.Now(),
            actor: PerformedByActor.User(UserId.FromGuid(request.ActorId)),
            correlationId: correlationId,
            causationId: causationId);

        await _incidentRepository.AddNoteAsync(aggregate, request.ActorId, request.CorrelationId, cancellationToken);

        _logger.LogInformation(
            "Note added to Incident {IncidentId} for Tenant {TenantId} CorrelationId {CorrelationId}",
            request.IncidentId, request.TenantId, request.CorrelationId);

        return request.IncidentId;
    }
}
