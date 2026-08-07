// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaGapsSummaryForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Queries;

/// <summary>Handles <see cref="GetNfpaGapsSummaryForSiteQuery"/>.</summary>
public sealed class GetNfpaGapsSummaryForSiteQueryHandler(
    INfpaAuditGapRepository repository,
    ILogger<GetNfpaGapsSummaryForSiteQueryHandler> logger)
    : IRequestHandler<GetNfpaGapsSummaryForSiteQuery, Result<IReadOnlyList<NfpaGapSummaryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaGapSummaryDto>>> Handle(GetNfpaGapsSummaryForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await repository.GetGapsSummaryForSiteAsync(request.TenantId, request.SiteId, request.Module, cancellationToken);
            return Result<IReadOnlyList<NfpaGapSummaryDto>>.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA gap summary for Site {SiteId} Module {Module} Tenant {TenantId}",
                request.SiteId, request.Module, request.TenantId);

            return Result<IReadOnlyList<NfpaGapSummaryDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.AuditGapsSummaryRetrievalFailed",
                "Unable to retrieve the NFPA audit gap summary at this time.",
                Guid.NewGuid()));
        }
    }
}
