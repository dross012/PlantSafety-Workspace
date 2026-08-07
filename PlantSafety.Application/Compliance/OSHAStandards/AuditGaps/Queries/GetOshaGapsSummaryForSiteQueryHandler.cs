// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaGapsSummaryForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Queries;

/// <summary>Handles <see cref="GetOshaGapsSummaryForSiteQuery"/>.</summary>
public sealed class GetOshaGapsSummaryForSiteQueryHandler(
    IOshaAuditGapRepository repository,
    ILogger<GetOshaGapsSummaryForSiteQueryHandler> logger)
    : IRequestHandler<GetOshaGapsSummaryForSiteQuery, Result<OshaGapSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<OshaGapSummaryDto>> Handle(GetOshaGapsSummaryForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await repository.GetGapsSummaryForSiteAsync(request.TenantId, request.SiteId, request.Module, cancellationToken);
            return Result<OshaGapSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA gap summary for Site {SiteId} Module {Module} Tenant {TenantId}",
                request.SiteId, request.Module, request.TenantId);

            return Result<OshaGapSummaryDto>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.AuditGapsSummaryRetrievalFailed",
                "Unable to retrieve the OSHA audit gap summary at this time.",
                Guid.NewGuid()));
        }
    }
}
