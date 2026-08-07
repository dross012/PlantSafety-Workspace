// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaSiteExposureQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetOshaSiteExposureQuery"/>.</summary>
public sealed class GetOshaSiteExposureQueryHandler(
    IOshaAuditGapRepository repository,
    ILogger<GetOshaSiteExposureQueryHandler> logger)
    : IRequestHandler<GetOshaSiteExposureQuery, Result<IReadOnlyList<OshaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OshaGapSummaryDto>>> Handle(GetOshaSiteExposureQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await repository.GetSiteExposureAsync(request.TenantId, request.SiteId, cancellationToken);
            return Result<IReadOnlyList<OshaGapSummaryDto>>.Success(exposure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA site exposure for Site {SiteId} Tenant {TenantId}", request.SiteId, request.TenantId);

            return Result<IReadOnlyList<OshaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.SiteExposureRetrievalFailed",
                "Unable to retrieve OSHA site exposure at this time.",
                Guid.NewGuid()));
        }
    }
}
