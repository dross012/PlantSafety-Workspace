// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-side repository abstraction for Tenant queries, mirroring the platform's
//   established IAuditReportReadRepository precedent.
// SchemaVersion: 1

using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Repositories;

/// <summary>Read-side repository abstraction for Tenant queries.</summary>
public interface ITenantReadRepository
{
    /// <summary>Retrieves a single Tenant's read model by identity, or <see langword="null"/> if none exists.</summary>
    Task<TenantDto?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Lists every registered tenant's read model, most recently created first. Platform-admin-only, cross-tenant operation.</summary>
    Task<IReadOnlyList<TenantDto>> ListAllAsync(CancellationToken cancellationToken);
}
