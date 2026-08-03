// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Infrastructure
//  File:           TenantDataExportStore.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.3 Tenant Lifecycle Management)
//  SchemaVersion:  2026.1
//  Description:    ITenantDataExportStore implementation backed by the platform's existing
//                  IBlobStorageService, mirroring AuditReportBlobContentStore's precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Tenancy.Tenant.Abstractions;
using PlantSafety.Infrastructure.FileStorage;

namespace PlantSafety.Infrastructure.Tenancy.Tenant.Persistence;

/// <summary>Blob-storage-backed implementation of <see cref="ITenantDataExportStore"/>.</summary>
public sealed class TenantDataExportStore(IBlobStorageService blobStorageService, ILogger<TenantDataExportStore> logger) : ITenantDataExportStore
{
    private const string ContainerName = "tenant-exports";

    /// <inheritdoc/>
    public async Task<string> SaveAsync(Guid tenantId, byte[] content, CancellationToken cancellationToken)
    {
        var blobName = $"{tenantId:N}/{DateTime.UtcNow:yyyyMMddHHmmss}.json";

        using var stream = new MemoryStream(content, writable: false);
        var uri = await blobStorageService.UploadAsync(ContainerName, blobName, stream, "application/json", cancellationToken);

        logger.LogInformation("Saved Tenant {TenantId} data export ({Bytes} bytes) to {Uri}", tenantId, content.LongLength, uri);
        return uri;
    }
}
