// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetDocumentsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Documents.Queries;
using PlantSafety.Application.Integrations.Documents.Repositories;

namespace PlantSafety.Application.Integrations.Documents.Handlers;

/// <summary>Handles <see cref="GetDocumentsQuery"/>.</summary>
public sealed class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, Result<DocumentsQueryResult>>
{
    private readonly IDocumentsRepository _repository;
    private readonly ILogger<GetDocumentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDocumentsQueryHandler(IDocumentsRepository repository, ILogger<GetDocumentsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DocumentsQueryResult>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDocumentsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<DocumentsQueryResult>.Success(new DocumentsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.Documents for Tenant {TenantId}", request.TenantId);

            return Result<DocumentsQueryResult>.Failure(new ApplicationError(
                "Integrations.Documents.RetrievalFailed",
                "Unable to retrieve synced document records at this time.",
                Guid.NewGuid()));
        }
    }
}
