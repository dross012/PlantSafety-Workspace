// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Infrastructure
//  File:           AuditReportBlobContentStore.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    IAuditReportContentStore backed by the platform's existing
//                  IBlobStorageService (Azure Blob Storage), reusing the same abstraction other
//                  file-carrying features already depend on rather than introducing a new storage
//                  mechanism. Rendered content is stored under a deterministic
//                  "audit-reports/{tenantId}/{auditReportId}.{ext}" blob path; the returned URI is
//                  persisted on the AuditReport aggregate as RenderedContentReference.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Application.Auditing.AuditReport.Repositories;
using PlantSafety.Infrastructure.FileStorage;

namespace PlantSafety.Infrastructure.Auditing.AuditReport.Persistence;

/// <summary>Blob-storage-backed implementation of <see cref="IAuditReportContentStore"/>.</summary>
public sealed class AuditReportBlobContentStore(IBlobStorageService blobStorageService, ILogger<AuditReportBlobContentStore> logger) : IAuditReportContentStore
{
    private const string ContainerName = "audit-reports";

    /// <inheritdoc/>
    public async Task<string> SaveAsync(Guid tenantId, Guid auditReportId, AuditReportFormat format, byte[] content, CancellationToken cancellationToken)
    {
        var blobName = $"{tenantId:N}/{auditReportId:N}.{format.Code.ToLowerInvariant()}";

        using var stream = new MemoryStream(content, writable: false);
        var uri = await blobStorageService.UploadAsync(ContainerName, blobName, stream, format.ContentType, cancellationToken);

        logger.LogInformation("Saved AuditReport {AuditReportId} content ({Bytes} bytes) to {Uri}", auditReportId, content.LongLength, uri);
        return uri;
    }

    /// <inheritdoc/>
    public async Task<byte[]?> GetAsync(string contentReference, CancellationToken cancellationToken)
    {
        // contentReference is the blob's URI as returned by SaveAsync; the container/blob name is
        // reconstructed from the last two URI path segments so this store round-trips its own
        // references without needing a separate lookup table.
        var uri = new Uri(contentReference, UriKind.RelativeOrAbsolute);
        var segments = uri.IsAbsoluteUri ? uri.AbsolutePath.Trim('/').Split('/') : contentReference.Trim('/').Split('/');
        if (segments.Length < 2) return null;

        var blobName = string.Join('/', segments[^2..]);

        try
        {
            await using var stream = await blobStorageService.DownloadAsync(ContainerName, blobName, cancellationToken);
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken);
            return buffer.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to download AuditReport content from {ContentReference}", contentReference);
            return null;
        }
    }
}
