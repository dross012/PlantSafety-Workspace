// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the RootCauseAnalysis aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.RootCauseAnalysis;

/// <summary>Repository for RootCauseAnalysis read/write operations.</summary>
public interface IRcaRepository
{
    /// <summary>Creates a new RCA; stages an outbox event atomically.</summary>
    Task CreateAsync(
        Guid rcaId,
        Guid tenantId,
        Guid linkedEventId,
        string linkedEventType,
        string methodology,
        string rootCauses,
        string correctiveActions,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<RcaReadModel?> GetByIdAsync(Guid tenantId, Guid rcaId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for RootCauseAnalysis stored-procedure result sets.</summary>
public sealed record RcaReadModel
{
    /// <summary>RCA identifier.</summary>
    public required Guid RcaId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Linked event identifier.</summary>
    public required Guid LinkedEventId { get; init; }
    /// <summary>Linked event type discriminator.</summary>
    public required string LinkedEventType { get; init; }
    /// <summary>Analysis methodology.</summary>
    public required string Methodology { get; init; }
    /// <summary>Root causes (serialised).</summary>
    public required string RootCauses { get; init; }
    /// <summary>Corrective actions (serialised).</summary>
    public required string CorrectiveActions { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
