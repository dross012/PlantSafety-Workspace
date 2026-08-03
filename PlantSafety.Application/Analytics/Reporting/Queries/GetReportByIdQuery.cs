// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve a single report's full detail by identifier. Backs the
//   PlantSafety.UI Reporting module's Report Detail page.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Analytics.Reporting.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Reporting.Queries;

/// <summary>Retrieves a single report's full detail by identifier.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ReportId">Identifier of the report to retrieve.</param>
public sealed record GetReportByIdQuery(Guid TenantId, Guid ReportId) : IRequest<Result<ReportDetailDto>>;
