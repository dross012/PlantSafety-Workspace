// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Query to generate an Operational data export and return a presigned download URL.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.Queries;

/// <summary>Generates an Operational data export in the requested format and returns a presigned download URL.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null exports data for all accessible sites.</param>
/// <param name="Format">Requested output format (0=Csv, 1=Excel, 2=Pdf).</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
/// <param name="Status">Optional status filter.</param>
/// <param name="MaxRecords">Maximum number of records to include.</param>
public sealed record ExportOperationalQuery(
    Guid TenantId,
    Guid? SiteId,
    int Format,
    DateTime? From,
    DateTime To,
    string? Status,
    int MaxRecords) : IRequest<Result<ExportOperationalResult>>;
