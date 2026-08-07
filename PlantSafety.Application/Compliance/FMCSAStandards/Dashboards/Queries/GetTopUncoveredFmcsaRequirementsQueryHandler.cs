// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetTopUncoveredFmcsaRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetTopUncoveredFmcsaRequirementsQuery"/>.</summary>
public sealed class GetTopUncoveredFmcsaRequirementsQueryHandler(
    IFmcsaAuditGapRepository repository,
    ILogger<GetTopUncoveredFmcsaRequirementsQueryHandler> logger)
    : IRequestHandler<GetTopUncoveredFmcsaRequirementsQuery, Result<IReadOnlyList<FmcsaGapDto>>>
{
    private const int MaxTake = 100;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaGapDto>>> Handle(GetTopUncoveredFmcsaRequirementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? 10 : request.Take;
            var gaps = await repository.GetTopUncoveredForSiteAsync(request.TenantId, request.SiteId, take, cancellationToken);
            return Result<IReadOnlyList<FmcsaGapDto>>.Success(gaps);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve top uncovered FMCSA requirements for Site {SiteId} Tenant {TenantId}", request.SiteId, request.TenantId);

            return Result<IReadOnlyList<FmcsaGapDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.TopUncoveredRetrievalFailed",
                "Unable to retrieve top uncovered FMCSA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
