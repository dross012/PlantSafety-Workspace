// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-07-27 -- Rewired through the Domain IncidentManagement aggregate
//   (IncidentManagement.Create) instead of pushing primitives straight to the repository, so
//   the aggregate's Create invariants (non-empty Tenant/Site/Title/Description/Severity/
//   Location/ReportedByWorkerId) are enforced here and IncidentManagementCreatedEvent is the
//   event actually raised/enqueued (IncidentDapperRepository.CreateAsync enqueues it to the
//   outbox after a successful write). See docs/architecture/plantsafety-architecture.md Sec.5.1.
// FILE DESCRIPTION: MediatR handler for CreateIncidentCommand.
//   Pipeline order: LoggingBehavior → MetricsBehavior → PerformanceBehavior → AuthorizationBehavior
//   → ValidationBehavior → ConcurrencyBehavior → TransactionBehavior → THIS HANDLER.
//   By the time this handler executes: auth is checked, inputs are validated, a transaction is open.
// SchemaVersion: 2

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Metrics;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using IncidentAggregate = PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="CreateIncidentCommand"/>.
/// Builds the <see cref="IncidentAggregate"/> via its <c>Create</c> domain factory (enforcing all
/// aggregate invariants), persists it via the domain repository, and stages an outbox event.
/// Returns the new <see cref="Guid"/> (UUIDv7) for the created incident.
/// </summary>
public sealed class CreateIncidentCommandHandler : ICommandHandler<CreateIncidentCommand, Guid>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateIncidentCommandHandler(
        IIncidentRepository incidentRepository,
        IDateTimeProvider clock,
        ILogger<CreateIncidentCommandHandler> logger)
    {
        _incidentRepository = incidentRepository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        var actorId = UserId.FromGuid(request.ActorId);
        var nowUtc = UtcTimestamp.From(_clock.UtcNow);

        var aggregate = IncidentAggregate.Create(
            createdBy: actorId,
            createdAtUtc: nowUtc,
            correlationId: CorrelationId.FromGuid(request.CorrelationId),
            causationId: CausationId.FromGuid(request.CausationId ?? request.CorrelationId),
            actor: PerformedByActor.User(actorId),
            tenantId: request.TenantId,
            siteId: request.SiteId,
            title: request.Title,
            description: request.Description,
            severity: request.Severity,
            location: request.Location,
            reportedByWorkerId: UserId.FromGuid(request.ReportedByWorkerId),
            incidentOccurredAtUtc: UtcTimestamp.From(request.IncidentOccurredAtUtc.UtcDateTime),
            assignedInvestigatorId: request.AssignedInvestigatorId is Guid investigatorId
                ? UserId.FromGuid(investigatorId)
                : null);

        await _incidentRepository.CreateAsync(
            aggregate: aggregate,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        // Outbox event (IncidentManagementCreatedEvent, raised by IncidentManagement.Create above)
        // is enqueued by IncidentDapperRepository.CreateAsync after the SQL write commits, and
        // flushed by TransactionalUnitOfWork.CommitAsync once this handler returns.

        var incidentId = aggregate.IncidentManagementId.Value;

        _logger.LogInformation(
            "Incident {IncidentId} created for Tenant {TenantId} by Actor {ActorId} CorrelationId {CorrelationId}",
            incidentId, request.TenantId, request.ActorId, request.CorrelationId);

        IncidentMetrics.IncidentsCreated.Add(1);

        return incidentId;
    }
}
