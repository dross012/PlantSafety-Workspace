// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRevisionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.Revisions.Queries;
using PlantSafety.Application.Compliance.Policy.Revisions.Repositories;

namespace PlantSafety.Application.Compliance.Policy.Revisions.Handlers;

/// <summary>Handles <see cref="GetRevisionsQuery"/>.</summary>
public sealed class GetRevisionsQueryHandler : IRequestHandler<GetRevisionsQuery, Result<RevisionsQueryResult>>
{
    private readonly IRevisionsRepository _repository;
    private readonly ILogger<GetRevisionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRevisionsQueryHandler(IRevisionsRepository repository, ILogger<GetRevisionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RevisionsQueryResult>> Handle(GetRevisionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRevisionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RevisionsQueryResult>.Success(new RevisionsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Revisions for Tenant {TenantId}", request.TenantId);

            return Result<RevisionsQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.Revisions.RetrievalFailed",
                "Unable to retrieve Revisions data at this time.",
                Guid.NewGuid()));
        }
    }
}
