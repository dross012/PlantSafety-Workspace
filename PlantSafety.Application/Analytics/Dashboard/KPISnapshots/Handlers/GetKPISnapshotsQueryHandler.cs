// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetKPISnapshotsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Queries;
using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Handlers;

/// <summary>Handles <see cref="GetKPISnapshotsQuery"/>.</summary>
public sealed class GetKPISnapshotsQueryHandler : IRequestHandler<GetKPISnapshotsQuery, Result<KPISnapshotsQueryResult>>
{
    private readonly IKPISnapshotsRepository _repository;
    private readonly ILogger<GetKPISnapshotsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetKPISnapshotsQueryHandler(IKPISnapshotsRepository repository, ILogger<GetKPISnapshotsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<KPISnapshotsQueryResult>> Handle(GetKPISnapshotsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetKpiSnapshotsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<KPISnapshotsQueryResult>.Success(new KPISnapshotsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve KPISnapshots for Tenant {TenantId}", request.TenantId);

            return Result<KPISnapshotsQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.KPISnapshots.RetrievalFailed",
                "Unable to retrieve KPISnapshots data at this time.",
                Guid.NewGuid()));
        }
    }
}
