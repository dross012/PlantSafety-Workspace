// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to download the rendered byte-stream content of a generated
//   AuditReport (the export flow's download step).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditReport.Queries;

/// <summary>Retrieves the rendered byte-stream content of a generated AuditReport.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AuditReportId">Identity of the report whose content should be downloaded.</param>
public sealed record GetAuditReportContentQuery(Guid TenantId, Guid AuditReportId) : IRequest<Result<AuditReportContentResult>>;

/// <summary>Rendered content payload returned by <see cref="GetAuditReportContentQuery"/>.</summary>
/// <param name="ContentType">MIME content type of <paramref name="Content"/>.</param>
/// <param name="FileName">Suggested download file name.</param>
/// <param name="Content">Raw rendered bytes.</param>
public sealed record AuditReportContentResult(string ContentType, string FileName, byte[] Content);
