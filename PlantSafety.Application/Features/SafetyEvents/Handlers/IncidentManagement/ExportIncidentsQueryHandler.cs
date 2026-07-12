// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: MediatR handler for ExportIncidentsQuery.
//   Fetches matching Incident records via the read-side repository and serialises them to
//   a CSV or JSON byte payload. Serialisation is an Application-layer concern; the repository
//   only performs data access.
// SchemaVersion: 1

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Export;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="ExportIncidentsQuery"/>.
/// Returns a CSV or JSON byte payload of matching <see cref="IncidentDto"/> records.
/// </summary>
public sealed class ExportIncidentsQueryHandler : IQueryHandler<ExportIncidentsQuery, byte[]>
{
    private const int AbsoluteMaxRecords = 10_000;

    private readonly IIncidentReadRepository _readRepository;
    private readonly IncidentToIncidentDtoMappingProfile _mapper;
    private readonly ILogger<ExportIncidentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExportIncidentsQueryHandler(
        IIncidentReadRepository readRepository,
        IncidentToIncidentDtoMappingProfile mapper,
        ILogger<ExportIncidentsQueryHandler> logger)
    {
        _readRepository = readRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<byte[]> Handle(ExportIncidentsQuery request, CancellationToken cancellationToken)
    {
        var maxRecords = Math.Clamp(request.MaxRecords, 1, AbsoluteMaxRecords);

        _logger.LogDebug(
            "Exporting Incidents for Tenant {TenantId} Format {Format} CorrelationId {CorrelationId}",
            request.TenantId, request.Format, request.CorrelationId);

        var readModels = await _readRepository.ExportAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            occurredFromUtc: request.OccurredFromUtc,
            occurredToUtc: request.OccurredToUtc,
            maxRecords: maxRecords,
            cancellationToken: cancellationToken);

        var items = readModels.Select(_mapper.Map).ToList();

        return string.Equals(request.Format, "json", StringComparison.OrdinalIgnoreCase)
            ? SerializeJson(items)
            : SerializeCsv(items);
    }

    private static byte[] SerializeJson(IReadOnlyList<IncidentDto> items) =>
        JsonSerializer.SerializeToUtf8Bytes(items);

    private static byte[] SerializeCsv(IReadOnlyList<IncidentDto> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("IncidentId,SiteId,Title,IncidentOccurredAtUtc,ReportedAtUtc,Severity,Status,Location,ReportedByWorkerId,AssignedInvestigatorId,ClosedAtUtc");

        foreach (var item in items)
        {
            sb.AppendLine(string.Join(',',
                item.IncidentId,
                item.SiteId,
                CsvEscape(item.Title),
                item.IncidentOccurredAtUtc.ToString("O"),
                item.ReportedAtUtc.ToString("O"),
                CsvEscape(item.Severity),
                CsvEscape(item.Status),
                CsvEscape(item.Location),
                item.ReportedByWorkerId,
                item.AssignedInvestigatorId,
                item.ClosedAtUtc?.ToString("O")));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string CsvEscape(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
