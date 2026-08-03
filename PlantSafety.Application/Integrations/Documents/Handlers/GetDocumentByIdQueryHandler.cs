// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetDocumentByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Documents.Models;
using PlantSafety.Application.Integrations.Documents.Queries;
using PlantSafety.Application.Integrations.Documents.Repositories;

namespace PlantSafety.Application.Integrations.Documents.Handlers;

/// <summary>Handles <see cref="GetDocumentByIdQuery"/>.</summary>
public sealed class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentsItemDto?>>
{
    private readonly IDocumentsRepository _repository;
    private readonly ILogger<GetDocumentByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDocumentByIdQueryHandler(IDocumentsRepository repository, ILogger<GetDocumentByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DocumentsItemDto?>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetDocumentByIdAsync(request.TenantId, request.DocumentId, cancellationToken);
            return Result<DocumentsItemDto?>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.Documents record {DocumentId} for Tenant {TenantId}", request.DocumentId, request.TenantId);

            return Result<DocumentsItemDto?>.Failure(new ApplicationError(
                "Integrations.Documents.DetailRetrievalFailed",
                "Unable to retrieve the document at this time.",
                Guid.NewGuid()));
        }
    }
}
