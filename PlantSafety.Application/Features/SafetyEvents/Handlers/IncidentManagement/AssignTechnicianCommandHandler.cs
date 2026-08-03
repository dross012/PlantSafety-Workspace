// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: MediatR handler for AssignTechnicianCommand.
//   Loads the current Incident, rehydrates the Domain aggregate, applies AssignTechnician
//   (enforcing lifecycle invariants), persists via IIncidentRepository.AssignAsync.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Assign;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>Handles <see cref="AssignTechnicianCommand"/>.</summary>
public sealed class AssignTechnicianCommandHandler : ICommandHandler<AssignTechnicianCommand, Guid>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AssignTechnicianCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AssignTechnicianCommandHandler(
        IIncidentRepository incidentRepository,
        IIncidentReadRepository incidentReadRepository,
        ICacheService cacheService,
        ILogger<AssignTechnicianCommandHandler> logger)
    {
        _incidentRepository = incidentRepository;
        _incidentReadRepository = incidentReadRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(AssignTechnicianCommand request, CancellationToken cancellationToken)
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

        aggregate.AssignTechnician(
            technicianId: UserId.FromGuid(request.TechnicianId),
            nowUtc: UtcTimestamp.Now(),
            actor: PerformedByActor.User(UserId.FromGuid(request.ActorId)),
            correlationId: correlationId,
            causationId: causationId);

        await _incidentRepository.AssignAsync(aggregate, request.ActorId, request.CorrelationId, cancellationToken);

        await _cacheService.RemoveAsync($"incidents:{request.TenantId}:{request.IncidentId}", cancellationToken);

        _logger.LogInformation(
            "Incident {IncidentId} assigned to technician {TechnicianId} for Tenant {TenantId} CorrelationId {CorrelationId}",
            request.IncidentId, request.TechnicianId, request.TenantId, request.CorrelationId);

        return request.IncidentId;
    }
}
