// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the EmergencyResponse aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;

/// <summary>Repository for EmergencyResponse read/write operations.</summary>
public interface IEmergencyResponseRepository
{
    /// <summary>Creates a new EmergencyResponse; stages an outbox event atomically.</summary>
    Task CreateAsync(
        Guid responseId,
        Guid tenantId,
        Guid siteId,
        Guid? linkedCrisisEventId,
        string responsePlanName,
        string description,
        DateTimeOffset activatedAtUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<EmergencyResponseReadModel?> GetByIdAsync(Guid tenantId, Guid responseId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for EmergencyResponse stored-procedure result sets.</summary>
public sealed record EmergencyResponseReadModel
{
    /// <summary>Response identifier.</summary>
    public required Guid ResponseId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Linked crisis event identifier.</summary>
    public required Guid? LinkedCrisisEventId { get; init; }
    /// <summary>Response plan name.</summary>
    public required string ResponsePlanName { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Activation timestamp.</summary>
    public required DateTimeOffset ActivatedAtUtc { get; init; }
    /// <summary>Stand-down timestamp.</summary>
    public required DateTimeOffset? StandDownAtUtc { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
