// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the ManagementOfChange aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Handlers.ManagementOfChange;

/// <summary>Repository for ManagementOfChange read/write operations.</summary>
public interface IMocRepository
{
    /// <summary>Creates a new MOC; stages a <c>MocCreatedEvent</c> in the outbox atomically.</summary>
    Task CreateAsync(
        Guid mocId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string changeType,
        string riskLevel,
        Guid initiatorId,
        DateTimeOffset proposedStartUtc,
        DateTimeOffset? proposedEndUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Approves a MOC; stages a <c>MocApprovedEvent</c>.</summary>
    Task ApproveAsync(
        Guid tenantId,
        Guid mocId,
        string approvalNotes,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<MocReadModel?> GetByIdAsync(Guid tenantId, Guid mocId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for ManagementOfChange stored-procedure result sets.</summary>
public sealed record MocReadModel
{
    /// <summary>MOC identifier.</summary>
    public required Guid MocId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Change type: Permanent/Temporary/Emergency.</summary>
    public required string ChangeType { get; init; }
    /// <summary>Risk level: Low/Medium/High/Critical.</summary>
    public required string RiskLevel { get; init; }
    /// <summary>Initiator worker identifier.</summary>
    public required Guid InitiatorId { get; init; }
    /// <summary>Proposed start timestamp.</summary>
    public required DateTimeOffset ProposedStartUtc { get; init; }
    /// <summary>Proposed end timestamp.</summary>
    public required DateTimeOffset? ProposedEndUtc { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
