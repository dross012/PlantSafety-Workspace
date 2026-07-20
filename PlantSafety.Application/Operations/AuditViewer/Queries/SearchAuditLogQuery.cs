// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to search the shared AuditLog table by optional filters, with pagination.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.AuditViewer.Models;

namespace PlantSafety.Application.Operations.AuditViewer.Queries;

/// <summary>Searches [PlantSafety.AuditDB].dbo.AuditLog by optional filters, newest first, paginated.</summary>
/// <param name="Criteria">The optional filter and pagination criteria.</param>
public sealed record SearchAuditLogQuery(AuditLogSearchCriteria Criteria) : IRequest<Result<AuditLogSearchResultDto>>;
