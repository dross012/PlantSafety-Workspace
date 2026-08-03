// AUTHOR: PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application-layer abstraction over the tenant's blob-storage usage, so the
//   Application layer (which must not reference PlantSafety.Infrastructure) can compose the
//   Storage Bytes metering dimension. Implemented in Infrastructure by wrapping the existing
//   IBlobStorageService.GetContainerSizeBytesAsync (Phase 7 §3.1 addition) -- mirrors the
//   established ITenantDataExportStore pattern already used elsewhere in Tenancy.Tenant.
// SchemaVersion: 1

namespace PlantSafety.Application.Billing.Metering.Abstractions;

/// <summary>Reports a tenant's current blob-storage consumption for usage-metering purposes.</summary>
public interface ITenantStorageUsageProvider
{
    /// <summary>Returns the tenant's total blob storage consumption, in bytes.</summary>
    Task<long> GetTenantStorageBytesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
