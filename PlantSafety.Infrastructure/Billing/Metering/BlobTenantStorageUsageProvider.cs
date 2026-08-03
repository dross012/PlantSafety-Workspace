// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Infrastructure
//  File:           BlobTenantStorageUsageProvider.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
//  SchemaVersion:  2026.1
//  Description:    Implements Application.Billing.Metering.Abstractions.ITenantStorageUsageProvider
//                  by wrapping the existing IBlobStorageService.GetContainerSizeBytesAsync against
//                  a per-tenant container naming convention. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Billing.Metering.Abstractions;
using PlantSafety.Infrastructure.FileStorage;

namespace PlantSafety.Infrastructure.Billing.Metering;

/// <summary><see cref="ITenantStorageUsageProvider"/> backed by the existing <see cref="IBlobStorageService"/>.</summary>
public sealed class BlobTenantStorageUsageProvider(IBlobStorageService blobStorageService) : ITenantStorageUsageProvider
{
    /// <summary>Container-naming convention for a tenant's blob storage.</summary>
    public static string ResolveTenantContainerName(Guid tenantId) => $"tenant-{tenantId:D}";

    /// <inheritdoc/>
    public Task<long> GetTenantStorageBytesAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => blobStorageService.GetContainerSizeBytesAsync(ResolveTenantContainerName(tenantId), cancellationToken);
}
