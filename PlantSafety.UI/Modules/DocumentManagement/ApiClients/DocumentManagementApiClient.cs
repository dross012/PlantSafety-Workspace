// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Document Management module.
//   Wraps all HTTP communication for the Documents List and Document
//   Detail pages. All methods return ApiResponseEnvelope<T>.
// SchemaVersion: 2026.1
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

    /// <summary>Retrieves a paginated page of documents.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<DocumentListItemDto>>> GetDocumentsAsync(
        int pageNumber = 1, int pageSize = 25, string? categoryFilter = null, string? search = null,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<DocumentListItemDto>>(
            $"v1/documents?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(categoryFilter) ? string.Empty : $"&category={Uri.EscapeDataString(categoryFilter)}") +
            (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail for a single document.</summary>
    public Task<ApiResponseEnvelope<DocumentDetailDto>> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
        => GetAsync<DocumentDetailDto>($"v1/documents/{documentId}", cancellationToken);
}
