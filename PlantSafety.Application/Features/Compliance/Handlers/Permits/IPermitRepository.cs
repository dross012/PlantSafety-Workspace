// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the Permit aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Handlers.Permits;

/// <summary>Repository for Permit read/write operations.</summary>
public interface IPermitRepository
{
    /// <summary>Creates a new Permit in Draft status; stages a <c>PermitCreatedEvent</c> in the outbox.</summary>
    Task CreateAsync(
        Guid permitId,
        Guid tenantId,
        Guid siteId,
        string permitType,
        string title,
        string description,
        Guid workLeadId,
        DateTimeOffset validFromUtc,
        DateTimeOffset validToUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Approves a permit; transitions status to Active; stages a <c>PermitApprovedEvent</c>.</summary>
    Task ApproveAsync(
        Guid tenantId,
        Guid permitId,
        string approvalNotes,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<PermitReadModel?> GetByIdAsync(Guid tenantId, Guid permitId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<PermitReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? permitType,
        string? status,
        DateTimeOffset? validFromUtc,
        DateTimeOffset? validToUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for Permit stored-procedure result sets.</summary>
public sealed record PermitReadModel
{
    /// <summary>Permit identifier.</summary>
    public required Guid PermitId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Permit type.</summary>
    public required string PermitType { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Work lead worker identifier.</summary>
    public required Guid WorkLeadId { get; init; }
    /// <summary>Valid from timestamp.</summary>
    public required DateTimeOffset ValidFromUtc { get; init; }
    /// <summary>Valid to timestamp.</summary>
    public required DateTimeOffset ValidToUtc { get; init; }
    /// <summary>Status: Draft/PendingApproval/Active/Suspended/Closed/Expired.</summary>
    public required string Status { get; init; }
    /// <summary>Approver actor identifier.</summary>
    public required Guid? ApprovedByActorId { get; init; }
    /// <summary>Approval timestamp.</summary>
    public required DateTimeOffset? ApprovedAtUtc { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
