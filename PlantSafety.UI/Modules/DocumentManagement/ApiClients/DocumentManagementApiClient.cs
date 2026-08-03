// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Document Management module.
//   Wraps all HTTP communication for the Documents List and Document
//   Detail pages. All methods return ApiResponseEnvelope<T>.
//   Rewired 2026-08-03 from the fictional v1/documents route onto the real,
//   mature Integrations.Documents module (v1/integrations/documents/list and
//   v1/integrations/documents/{id}) per docs/Phase5/ui-gaps-scoping.md.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.DocumentManagement.Models;

namespace PlantSafety.UI.Modules.DocumentManagement.ApiClients;

/// <summary>Typed API client for the Document Management module.</summary>
public sealed class DocumentManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="DocumentManagementApiClient"/>.</summary>
    public DocumentManagementApiClient(HttpClient http, ILogger<DocumentManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated page of Integrations.Documents records for the active tenant.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="categoryFilter">Optional document-type ("category") filter.</param>
    /// <param name="search">Optional free-text search matched against the file name.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<DocumentListItemDto>>> GetDocumentsAsync(
        Guid tenantId, int pageNumber = 1, int pageSize = 25, string? categoryFilter = null, string? search = null,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<DocumentListItemDto>>(
            $"v1/integrations/documents/list?tenantId={Uri.EscapeDataString(tenantId.ToString())}" +
            $"&pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(categoryFilter) ? string.Empty : $"&category={Uri.EscapeDataString(categoryFilter)}") +
            (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail for a single Integrations.Documents record.</summary>
    /// <param name="documentId">The synced document record identifier.</param>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<DocumentDetailDto>> GetDocumentAsync(
        Guid documentId, Guid tenantId, CancellationToken cancellationToken = default)
        => GetAsync<DocumentDetailDto>(
            $"v1/integrations/documents/{documentId}?tenantId={Uri.EscapeDataString(tenantId.ToString())}",
            cancellationToken);
}
