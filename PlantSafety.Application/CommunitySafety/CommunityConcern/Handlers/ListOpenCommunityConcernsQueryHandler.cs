// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListOpenCommunityConcernsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Repositories;

namespace PlantSafety.Application.CommunitySafety.CommunityConcern.Handlers;

/// <summary>Handles <see cref="ListOpenCommunityConcernsQuery"/>.</summary>
public sealed class ListOpenCommunityConcernsQueryHandler : IRequestHandler<ListOpenCommunityConcernsQuery, Result<CommunityConcernsQueryResult>>
{
    private readonly ICommunityConcernRepository _repository;
    private readonly ILogger<ListOpenCommunityConcernsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListOpenCommunityConcernsQueryHandler(ICommunityConcernRepository repository, ILogger<ListOpenCommunityConcernsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityConcernsQueryResult>> Handle(ListOpenCommunityConcernsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetOpenCommunityConcernItemsAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<CommunityConcernsQueryResult>.Success(new CommunityConcernsQueryResult(
                request.TenantId, request.SiteId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list open CommunityConcern records for Tenant {TenantId}", request.TenantId);

            return Result<CommunityConcernsQueryResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityConcern.RetrievalFailed",
                "Unable to list open CommunityConcern records at this time.",
                Guid.NewGuid()));
        }
    }
}
