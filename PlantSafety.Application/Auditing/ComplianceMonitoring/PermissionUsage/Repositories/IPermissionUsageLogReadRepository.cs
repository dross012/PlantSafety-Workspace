// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 7 §5.4)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-side repository for the tenant-scoped access-log/permission-usage audit
//                    stream (Handbook Phase 6 §5.4 Compliance Monitoring). Companion to the
//                    existing write-only IPermissionUsageLogRepository -- kept as a separate
//                    interface (Handbook §2.10 Repositories Architectural Rule: read/write
//                    segregation) rather than adding a read method to the append-only write
//                    contract.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;

/// <summary>Read-side repository over the tenant-scoped authorization-decision (permission usage) audit stream.</summary>
public interface IPermissionUsageLogReadRepository
{
    /// <summary>
    /// Retrieves every recorded authorization decision for <paramref name="tenantId"/> within the
    /// inclusive UTC range [<paramref name="fromUtc"/>, <paramref name="toUtc"/>], newest first.
    /// Never returns another tenant's entries -- TenantId is the mandatory filter, not an optional
    /// one, so this method cannot be called in a way that returns cross-tenant data.
    /// </summary>
    Task<IReadOnlyList<PermissionUsageLogEntry>> GetByTenantAsync(
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken);
}
