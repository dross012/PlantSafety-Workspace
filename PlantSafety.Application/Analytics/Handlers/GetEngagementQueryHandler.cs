// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for GetEngagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Analytics.Queries;
using PlantSafety.Application.Analytics.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Handlers;

/// <summary>Handles <see cref="GetEngagementQuery"/>.</summary>
public sealed class GetEngagementQueryHandler : IRequestHandler<GetEngagementQuery, Result<EngagementQueryResult>>
{
    private readonly IEngagementRepository _repository;
    private readonly ILogger<GetEngagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEngagementQueryHandler(IEngagementRepository repository, ILogger<GetEngagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EngagementQueryResult>> Handle(GetEngagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _repository.GetDataPointsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EngagementQueryResult>.Success(new EngagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Engagement analytics for Tenant {TenantId}", request.TenantId);

            return Result<EngagementQueryResult>.Failure(new ApplicationError(
                "Analytics.Engagement.RetrievalFailed",
                "Unable to retrieve engagement analytics at this time.",
                Guid.NewGuid()));
        }
    }
}
