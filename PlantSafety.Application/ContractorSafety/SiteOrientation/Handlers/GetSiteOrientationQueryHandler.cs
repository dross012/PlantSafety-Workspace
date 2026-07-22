// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSiteOrientationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContractorSafety.SiteOrientation.Queries;
using PlantSafety.Application.ContractorSafety.SiteOrientation.Repositories;

namespace PlantSafety.Application.ContractorSafety.SiteOrientation.Handlers;

/// <summary>Handles <see cref="GetSiteOrientationQuery"/>.</summary>
public sealed class GetSiteOrientationQueryHandler : IRequestHandler<GetSiteOrientationQuery, Result<SiteOrientationQueryResult>>
{
    private readonly ISiteOrientationRepository _repository;
    private readonly ILogger<GetSiteOrientationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSiteOrientationQueryHandler(ISiteOrientationRepository repository, ILogger<GetSiteOrientationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SiteOrientationQueryResult>> Handle(GetSiteOrientationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSiteOrientationItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SiteOrientationQueryResult>.Success(new SiteOrientationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SiteOrientation records for Tenant {TenantId}", request.TenantId);

            return Result<SiteOrientationQueryResult>.Failure(new ApplicationError(
                "ContractorSafety.SiteOrientation.RetrievalFailed",
                "Unable to retrieve SiteOrientation records at this time.",
                Guid.NewGuid()));
        }
    }
}