// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaExposureTrendQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetNfpaExposureTrendQuery"/>.</summary>
public sealed class GetNfpaExposureTrendQueryHandler(
    INfpaAuditGapRepository repository,
    ILogger<GetNfpaExposureTrendQueryHandler> logger)
    : IRequestHandler<GetNfpaExposureTrendQuery, Result<IReadOnlyList<NfpaExposureSnapshotDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaExposureSnapshotDto>>> Handle(GetNfpaExposureTrendQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var trend = await repository.GetExposureTrendAsync(
                request.TenantId, request.SiteId, request.Module, request.FromUtc, request.ToUtc, cancellationToken);
            return Result<IReadOnlyList<NfpaExposureSnapshotDto>>.Success(trend);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA exposure trend for Module {Module} Tenant {TenantId}", request.Module, request.TenantId);

            return Result<IReadOnlyList<NfpaExposureSnapshotDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.ExposureTrendRetrievalFailed",
                "Unable to retrieve the NFPA exposure trend at this time.",
                Guid.NewGuid()));
        }
    }
}
