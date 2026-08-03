// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to retrieve a single generated AuditReport by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditReport.Models;

namespace PlantSafety.Application.Auditing.AuditReport.Queries;

/// <summary>Retrieves a single AuditReport by identity, scoped to the tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AuditReportId">Identity of the report to retrieve.</param>
public sealed record GetAuditReportQuery(Guid TenantId, Guid AuditReportId) : IRequest<Result<AuditReportDto>>;
