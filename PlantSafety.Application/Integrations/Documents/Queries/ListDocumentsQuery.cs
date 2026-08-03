// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve a paginated, optionally category(document-type)- and
//   search-filtered list of Integrations.Documents read-model records for a tenant/site.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Documents.Models;

namespace PlantSafety.Application.Integrations.Documents.Queries;

/// <summary>
/// Retrieves a paginated page of Integrations.Documents records for the given tenant, optionally
/// scoped to a single site and filtered by document type ("category") and/or a free-text search
/// term matched against the file name.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="DocumentType">Optional document-type ("category") filter (e.g. Policy, Procedure, Training, Compliance).</param>
/// <param name="Search">Optional free-text search matched against the synced document's file name.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page, capped by <see cref="Common.Constants.PaginationConstants"/>.</param>
public sealed record ListDocumentsQuery(
    Guid TenantId,
    Guid? SiteId,
    string? DocumentType,
    string? Search,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<DocumentsItemDto>>>;
