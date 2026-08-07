// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaTenantExposureQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetOshaTenantExposureQuery"/>.</summary>
public sealed class GetOshaTenantExposureQueryHandler(
    IOshaAuditGapRepository repository,
    ILogger<GetOshaTenantExposureQueryHandler> logger)
    : IRequestHandler<GetOshaTenantExposureQuery, Result<IReadOnlyList<OshaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OshaGapSummaryDto>>> Handle(GetOshaTenantExposureQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await repository.GetTenantExposureAsync(request.TenantId, cancellationToken);
            return Result<IReadOnlyList<OshaGapSummaryDto>>.Success(exposure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA tenant exposure for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<OshaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.TenantExposureRetrievalFailed",
                "Unable to retrieve OSHA tenant exposure at this time.",
                Guid.NewGuid()));
        }
    }
}
