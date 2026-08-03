// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ListDocumentsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Documents.Models;
using PlantSafety.Application.Integrations.Documents.Queries;
using PlantSafety.Application.Integrations.Documents.Repositories;

namespace PlantSafety.Application.Integrations.Documents.Handlers;

/// <summary>Handles <see cref="ListDocumentsQuery"/>.</summary>
public sealed class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, Result<PaginatedResult<DocumentsItemDto>>>
{
    private readonly IDocumentsRepository _repository;
    private readonly ILogger<ListDocumentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListDocumentsQueryHandler(IDocumentsRepository repository, ILogger<ListDocumentsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<DocumentsItemDto>>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber < 1 ? PaginationConstants.DefaultPageNumber : request.PageNumber;
            var pageSize = request.PageSize < PaginationConstants.MinPageSize
                ? PaginationConstants.DefaultPageSize
                : Math.Min(request.PageSize, PaginationConstants.MaxPageSize);

            var (items, totalCount) = await _repository.ListDocumentsAsync(
                request.TenantId, request.SiteId, request.DocumentType, request.Search, pageNumber, pageSize, cancellationToken);

            return Result<PaginatedResult<DocumentsItemDto>>.Success(new PaginatedResult<DocumentsItemDto>(items, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve paginated Integrations.Documents list for Tenant {TenantId}", request.TenantId);

            return Result<PaginatedResult<DocumentsItemDto>>.Failure(new ApplicationError(
                "Integrations.Documents.ListRetrievalFailed",
                "Unable to retrieve the document list at this time.",
                Guid.NewGuid()));
        }
    }
}
