// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaExposureForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Handles <see cref="GetNfpaExposureForSiteQuery"/>.</summary>
public sealed class GetNfpaExposureForSiteQueryHandler(
    INfpaAuditGapRepository repository,
    ILogger<GetNfpaExposureForSiteQueryHandler> logger)
    : IRequestHandler<GetNfpaExposureForSiteQuery, Result<IReadOnlyList<NfpaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaGapSummaryDto>>> Handle(GetNfpaExposureForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await repository.GetSiteExposureAsync(request.TenantId, request.SiteId, cancellationToken);
            return Result<IReadOnlyList<NfpaGapSummaryDto>>.Success(exposure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA exposure for Site {SiteId} Tenant {TenantId}", request.SiteId, request.TenantId);

            return Result<IReadOnlyList<NfpaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.ExposureForSiteRetrievalFailed",
                "Unable to retrieve NFPA exposure for this site at this time.",
                Guid.NewGuid()));
        }
    }
}
