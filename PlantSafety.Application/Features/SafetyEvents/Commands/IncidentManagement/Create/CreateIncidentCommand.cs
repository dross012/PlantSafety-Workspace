// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a new Incident record.
//   Returns the UUIDv7 of the newly created incident.
//   Pipeline: LoggingBehavior → MetricsBehavior → PerformanceBehavior → AuthorizationBehavior
//             → ValidationBehavior → ConcurrencyBehavior → TransactionBehavior → Handler
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;

/// <summary>
/// Creates a new Incident in the SafetyEvents module.
/// The caller must hold the <c>SafetyEvents.Incidents.Create</c> permission.
/// On success, an <c>IncidentCreatedEvent</c> is placed in the outbox atomically.
/// </summary>
public sealed record CreateIncidentCommand : ICommand<Guid>
{
    // --- Canonical envelope (required by ICommand) ---

    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid? CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    // --- Domain payload ---

    /// <summary>Site where the incident occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short descriptive title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed narrative description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp when the incident actually occurred.</summary>
    public required DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>Initial severity classification (e.g., Minor, Moderate, Serious, Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Physical or logical location description. Max 500 characters.</summary>
    public required string Location { get; init; }

    /// <summary>Worker who is reporting the incident.</summary>
    public required Guid ReportedByWorkerId { get; init; }

    /// <summary>Worker assigned as investigator, if known at time of creation.</summary>
    public Guid? AssignedInvestigatorId { get; init; }
}
