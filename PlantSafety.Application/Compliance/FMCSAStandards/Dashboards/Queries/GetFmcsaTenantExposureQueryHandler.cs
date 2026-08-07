// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaTenantExposureQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetFmcsaTenantExposureQuery"/>.</summary>
public sealed class GetFmcsaTenantExposureQueryHandler(
    IFmcsaAuditGapRepository repository,
    ILogger<GetFmcsaTenantExposureQueryHandler> logger)
    : IRequestHandler<GetFmcsaTenantExposureQuery, Result<IReadOnlyList<FmcsaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaGapSummaryDto>>> Handle(GetFmcsaTenantExposureQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await repository.GetTenantExposureAsync(request.TenantId, cancellationToken);
            return Result<IReadOnlyList<FmcsaGapSummaryDto>>.Success(exposure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA tenant exposure for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<FmcsaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.TenantExposureRetrievalFailed",
                "Unable to retrieve FMCSA tenant exposure at this time.",
                Guid.NewGuid()));
        }
    }
}
