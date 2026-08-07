// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaGapsSummaryForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Queries;

/// <summary>Handles <see cref="GetFmcsaGapsSummaryForSiteQuery"/>.</summary>
public sealed class GetFmcsaGapsSummaryForSiteQueryHandler(
    IFmcsaAuditGapRepository repository,
    ILogger<GetFmcsaGapsSummaryForSiteQueryHandler> logger)
    : IRequestHandler<GetFmcsaGapsSummaryForSiteQuery, Result<IReadOnlyList<FmcsaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaGapSummaryDto>>> Handle(GetFmcsaGapsSummaryForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await repository.GetGapsSummaryForSiteAsync(request.TenantId, request.SiteId, request.Module, cancellationToken);
            return Result<IReadOnlyList<FmcsaGapSummaryDto>>.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA gap summary for Site {SiteId} Module {Module} Tenant {TenantId}",
                request.SiteId, request.Module, request.TenantId);

            return Result<IReadOnlyList<FmcsaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.AuditGapsSummaryRetrievalFailed",
                "Unable to retrieve the FMCSA audit gap summary at this time.",
                Guid.NewGuid()));
        }
    }
}
