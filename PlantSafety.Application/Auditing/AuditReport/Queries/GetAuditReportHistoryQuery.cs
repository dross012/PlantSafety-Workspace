// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to retrieve the AuditReport generation history for a target entity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditReport.Models;

namespace PlantSafety.Application.Auditing.AuditReport.Queries;

/// <summary>Retrieves the AuditReport generation history for a target entity, most recent first.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="TargetEntityType">Type discriminator of the entity.</param>
/// <param name="TargetEntityId">Identity of the entity.</param>
public sealed record GetAuditReportHistoryQuery(
    Guid TenantId, string TargetEntityType, Guid TargetEntityId) : IRequest<Result<IReadOnlyList<AuditReportDto>>>;
