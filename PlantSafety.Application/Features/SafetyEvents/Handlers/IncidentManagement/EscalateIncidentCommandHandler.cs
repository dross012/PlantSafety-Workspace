// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: MediatR handler for EscalateIncidentCommand.
//   Loads the current Incident, rehydrates the Domain aggregate, applies Escalate (severity bump
//   + lifecycle invariants), persists via IIncidentRepository.EscalateAsync.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Escalate;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>Handles <see cref="EscalateIncidentCommand"/>.</summary>
public sealed class EscalateIncidentCommandHandler : ICommandHandler<EscalateIncidentCommand, Guid>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<EscalateIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EscalateIncidentCommandHandler(
        IIncidentRepository incidentRepository,
        IIncidentReadRepository incidentReadRepository,
        ICacheService cacheService,
        ILogger<EscalateIncidentCommandHandler> logger)
    {
        _incidentRepository = incidentRepository;
        _incidentReadRepository = incidentReadRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(EscalateIncidentCommand request, CancellationToken cancellationToken)
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

        aggregate.Escalate(
            reason: request.Reason,
            nowUtc: UtcTimestamp.Now(),
            actor: PerformedByActor.User(UserId.FromGuid(request.ActorId)),
            correlationId: correlationId,
            causationId: causationId);

        await _incidentRepository.EscalateAsync(aggregate, request.ActorId, request.CorrelationId, cancellationToken);

        await _cacheService.RemoveAsync($"incidents:{request.TenantId}:{request.IncidentId}", cancellationToken);

        _logger.LogInformation(
            "Incident {IncidentId} escalated for Tenant {TenantId} CorrelationId {CorrelationId}",
            request.IncidentId, request.TenantId, request.CorrelationId);

        return request.IncidentId;
    }
}
