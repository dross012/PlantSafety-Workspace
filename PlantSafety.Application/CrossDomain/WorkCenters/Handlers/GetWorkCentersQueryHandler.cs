// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetWorkCentersQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.WorkCenters.Queries;
using PlantSafety.Application.CrossDomain.WorkCenters.Repositories;

namespace PlantSafety.Application.CrossDomain.WorkCenters.Handlers;

/// <summary>Handles <see cref="GetWorkCentersQuery"/>.</summary>
public sealed class GetWorkCentersQueryHandler : IRequestHandler<GetWorkCentersQuery, Result<WorkCentersQueryResult>>
{
    private readonly IWorkCentersRepository _repository;
    private readonly ILogger<GetWorkCentersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkCentersQueryHandler(IWorkCentersRepository repository, ILogger<GetWorkCentersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkCentersQueryResult>> Handle(GetWorkCentersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWorkCentersAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WorkCentersQueryResult>.Success(new WorkCentersQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkCenters for Tenant {TenantId}", request.TenantId);

            return Result<WorkCentersQueryResult>.Failure(new ApplicationError(
                "CrossDomain.WorkCenters.RetrievalFailed",
                "Unable to retrieve work centers data at this time.",
                Guid.NewGuid()));
        }
    }
}
