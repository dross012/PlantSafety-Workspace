// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaExposureForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Handles <see cref="GetFmcsaExposureForSiteQuery"/>.</summary>
public sealed class GetFmcsaExposureForSiteQueryHandler(
    IFmcsaAuditGapRepository repository,
    ILogger<GetFmcsaExposureForSiteQueryHandler> logger)
    : IRequestHandler<GetFmcsaExposureForSiteQuery, Result<IReadOnlyList<FmcsaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaGapSummaryDto>>> Handle(GetFmcsaExposureForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await repository.GetSiteExposureAsync(request.TenantId, request.SiteId, cancellationToken);
            return Result<IReadOnlyList<FmcsaGapSummaryDto>>.Success(exposure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA exposure for Site {SiteId} Tenant {TenantId}", request.SiteId, request.TenantId);

            return Result<IReadOnlyList<FmcsaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.SiteExposureRetrievalFailed",
                "Unable to retrieve FMCSA site exposure at this time.",
                Guid.NewGuid()));
        }
    }
}
