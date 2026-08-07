// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaTenantExposureQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetNfpaTenantExposureQuery"/>.</summary>
public sealed class GetNfpaTenantExposureQueryHandler(
    INfpaAuditGapRepository repository,
    ILogger<GetNfpaTenantExposureQueryHandler> logger)
    : IRequestHandler<GetNfpaTenantExposureQuery, Result<IReadOnlyList<NfpaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaGapSummaryDto>>> Handle(GetNfpaTenantExposureQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await repository.GetTenantExposureAsync(request.TenantId, cancellationToken);
            return Result<IReadOnlyList<NfpaGapSummaryDto>>.Success(exposure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA tenant exposure for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<NfpaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.TenantExposureRetrievalFailed",
                "Unable to retrieve NFPA tenant exposure at this time.",
                Guid.NewGuid()));
        }
    }
}
