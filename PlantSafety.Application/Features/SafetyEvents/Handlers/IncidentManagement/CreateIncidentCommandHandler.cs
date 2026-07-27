// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR handler for CreateIncidentCommand.
//   Pipeline order: LoggingBehavior → MetricsBehavior → PerformanceBehavior → AuthorizationBehavior
//   → ValidationBehavior → ConcurrencyBehavior → TransactionBehavior → THIS HANDLER.
//   By the time this handler executes: auth is checked, inputs are validated, a transaction is open.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Events.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Metrics;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="CreateIncidentCommand"/>.
/// Creates a new Incident record via the domain repository and stages an outbox event.
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
        var incidentId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _incidentRepository.CreateAsync(
            incidentId: incidentId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            title: request.Title,
            description: request.Description,
            incidentOccurredAtUtc: request.IncidentOccurredAtUtc,
            severity: request.Severity,
            location: request.Location,
            reportedByWorkerId: request.ReportedByWorkerId,
            assignedInvestigatorId: request.AssignedInvestigatorId,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        // Outbox event is written by the repository (via UoW) before the transaction commits.
        // The Outbox Processor will publish IncidentCreatedEvent to Azure Service Bus asynchronously.

        _logger.LogInformation(
            "Incident {IncidentId} created for Tenant {TenantId} by Actor {ActorId} CorrelationId {CorrelationId}",
            incidentId, request.TenantId, request.ActorId, request.CorrelationId);

        IncidentMetrics.IncidentsCreated.Add(1);

        return incidentId;
    }
}
