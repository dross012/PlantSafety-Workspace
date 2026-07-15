// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.AnalyticsExportItems
//  Layer:          Infrastructure
//  File:           ExportAnalyticsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper + Blob Storage backed implementation of
//                  IExportAnalyticsRepository. Queries the cross-cutting
//                  [Analytics].[AnalyticsExportItems] staging table, renders a
//                  delimited file for the requested format, uploads it via the
//                  existing IBlobStorageService abstraction, and returns download
//                  metadata. Mirrors ExportComplianceDapperRepository.
//
//                  ASSUMPTION (documented per pilot-pass instructions): no
//                  presigned/SAS URL generator exists anywhere in Infrastructure
//                  today, so the canonical blob URI returned by
//                  IBlobStorageService.UploadAsync is used as the DownloadUrl and
//                  ExpiresAt is a nominal +24h marker rather than a cryptographically
//                  enforced SAS expiry. True format-specific rendering (Excel/PDF)
//                  is out of scope for this pass: all formats currently render as
//                  delimited text with a format-appropriate file name/content type;
//                  wiring a real XLSX/PDF renderer is left for a follow-up pass.
//                  Additive-only evolution.
// ============================================================================
using System.Text;
using PlantSafety.Application.Analytics.Queries;
using PlantSafety.Application.Analytics.Repositories;
using PlantSafety.Infrastructure.FileStorage;

namespace PlantSafety.Infrastructure.Analytics.Persistence;

/// <summary>
/// Dapper + Blob Storage backed implementation of <see cref="IExportAnalyticsRepository"/>.
/// </summary>
public sealed class ExportAnalyticsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    IBlobStorageService blobStorageService,
    ILogger<ExportAnalyticsDapperRepository> logger)
    : IExportAnalyticsRepository
{
    private const string ContainerName = "analytics-exports";

    /// <inheritdoc/>
    public async Task<ExportAnalyticsResult> GenerateExportAsync(
        Guid tenantId,
        Guid? siteId,
        int format,
        DateTime? from,
        DateTime to,
        string? status,
        int maxRecords,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GenerateExportAsync for tenant {TenantId}, format {Format}", tenantId, format);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Status", status, 50)
            .AddInt("MaxRecords", maxRecords)
            .Build();

        var rows = await queryExecutor.QueryAsync<AnalyticsExportRow>(
            "[Analytics].[usp_AnalyticsExportItems_List]", parameters, cancellationToken);

        var (extension, contentType) = ResolveFormat(format);
        var fileName = $"analytics-export-{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
        var blobName = $"{tenantId}/{Guid.NewGuid()}-{fileName}";

        var content = BuildDelimitedContent(rows);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var blobUri = await blobStorageService.UploadAsync(ContainerName, blobName, stream, contentType, cancellationToken);

        logger.LogInformation(
            "Generated Analytics export {FileName} with {RecordCount} records for tenant {TenantId}",
            fileName, rows.Count, tenantId);

        return new ExportAnalyticsResult(
            fileName,
            contentType,
            blobUri,
            DateTimeOffset.UtcNow.AddHours(24),
            rows.Count);
    }

    /// <summary>Maps the numeric export format (0=Csv, 1=Excel, 2=Pdf) to a file extension and MIME type.</summary>
    private static (string Extension, string ContentType) ResolveFormat(int format) => format switch
    {
        1 => ("xls", "application/vnd.ms-excel"),
        2 => ("pdf", "application/pdf"),
        _ => ("csv", "text/csv"),
    };

    /// <summary>Renders export rows as delimited (CSV-shaped) text content.</summary>
    private static string BuildDelimitedContent(IReadOnlyList<AnalyticsExportRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,ResourceType,ResourceId,Title,Status,EffectiveAtUtc,OwnerId,SiteId");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(',',
                row.Id,
                Escape(row.ResourceType),
                row.ResourceId,
                Escape(row.Title),
                Escape(row.Status),
                row.EffectiveAtUtc.ToString("O"),
                row.OwnerId,
                row.SiteId));
        }

        return sb.ToString();
    }

    /// <summary>Escapes a field for CSV output by quoting values containing commas or quotes.</summary>
    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    /// <summary>Row shape returned by <c>[Analytics].[usp_AnalyticsExportItems_List]</c>.</summary>
    private sealed record AnalyticsExportRow
    {
        public required Guid Id { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ResourceType { get; init; }
        public required Guid ResourceId { get; init; }
        public required string Title { get; init; }
        public required string Status { get; init; }
        public required DateTime EffectiveAtUtc { get; init; }
        public required Guid OwnerId { get; init; }
    }
}
