// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetTopUncoveredNfpaHazardsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetTopUncoveredNfpaHazardsQuery"/>.</summary>
public sealed class GetTopUncoveredNfpaHazardsQueryHandler(
    INfpaAuditGapRepository repository,
    ILogger<GetTopUncoveredNfpaHazardsQueryHandler> logger)
    : IRequestHandler<GetTopUncoveredNfpaHazardsQuery, Result<IReadOnlyList<NfpaGapDto>>>
{
    private const int MaxTake = 100;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaGapDto>>> Handle(GetTopUncoveredNfpaHazardsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? 10 : request.Take;
            var gaps = await repository.GetTopUncoveredForSiteAsync(request.TenantId, request.SiteId, take, cancellationToken);
            return Result<IReadOnlyList<NfpaGapDto>>.Success(gaps);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve top uncovered NFPA hazards for Site {SiteId} Tenant {TenantId}", request.SiteId, request.TenantId);

            return Result<IReadOnlyList<NfpaGapDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.TopUncoveredRetrievalFailed",
                "Unable to retrieve top uncovered NFPA hazards at this time.",
                Guid.NewGuid()));
        }
    }
}
