// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application-layer storage abstraction for ExportTenantDataCommand's rendered
//   JSON export artifact, mirroring the platform's established IAuditReportContentStore pattern.
// SchemaVersion: 1

namespace PlantSafety.Application.Tenancy.Tenant.Abstractions;

/// <summary>Persists a tenant's exported data artifact.</summary>
public interface ITenantDataExportStore
{
    /// <summary>Saves the exported JSON content for <paramref name="tenantId"/>, returning a storage reference.</summary>
    Task<string> SaveAsync(Guid tenantId, byte[] content, CancellationToken cancellationToken);
}
