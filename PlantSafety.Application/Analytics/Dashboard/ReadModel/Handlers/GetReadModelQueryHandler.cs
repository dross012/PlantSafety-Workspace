// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetReadModelQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.ReadModel.Queries;
using PlantSafety.Application.Analytics.Dashboard.ReadModel.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.ReadModel.Handlers;

/// <summary>Handles <see cref="GetReadModelQuery"/>.</summary>
public sealed class GetReadModelQueryHandler : IRequestHandler<GetReadModelQuery, Result<ReadModelQueryResult>>
{
    private readonly IReadModelRepository _repository;
    private readonly ILogger<GetReadModelQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetReadModelQueryHandler(IReadModelRepository repository, ILogger<GetReadModelQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ReadModelQueryResult>> Handle(GetReadModelQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDashboardRowsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ReadModelQueryResult>.Success(new ReadModelQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ReadModel for Tenant {TenantId}", request.TenantId);

            return Result<ReadModelQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.ReadModel.RetrievalFailed",
                "Unable to retrieve ReadModel data at this time.",
                Guid.NewGuid()));
        }
    }
}
