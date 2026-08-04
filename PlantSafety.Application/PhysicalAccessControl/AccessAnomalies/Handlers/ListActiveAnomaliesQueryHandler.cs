// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListActiveAnomaliesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Repositories;

namespace PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Handlers;

/// <summary>Handles <see cref="ListActiveAnomaliesQuery"/>.</summary>
public sealed class ListActiveAnomaliesQueryHandler : IRequestHandler<ListActiveAnomaliesQuery, Result<ListActiveAnomaliesResult>>
{
    private readonly IAccessAnomalyReadRepository _repository;
    private readonly ILogger<ListActiveAnomaliesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListActiveAnomaliesQueryHandler(IAccessAnomalyReadRepository repository, ILogger<ListActiveAnomaliesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListActiveAnomaliesResult>> Handle(ListActiveAnomaliesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListActiveAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<ListActiveAnomaliesResult>.Success(new ListActiveAnomaliesResult(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list active AccessAnomalies for Tenant {TenantId}", request.TenantId);

            return Result<ListActiveAnomaliesResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessAnomaly.RetrievalFailed",
                "Unable to retrieve active AccessAnomaly records at this time.",
                Guid.NewGuid()));
        }
    }
}
