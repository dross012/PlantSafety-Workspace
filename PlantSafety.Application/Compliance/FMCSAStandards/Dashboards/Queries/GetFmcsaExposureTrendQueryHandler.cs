// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaExposureTrendQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetFmcsaExposureTrendQuery"/>.</summary>
public sealed class GetFmcsaExposureTrendQueryHandler(
    IFmcsaAuditGapRepository repository,
    ILogger<GetFmcsaExposureTrendQueryHandler> logger)
    : IRequestHandler<GetFmcsaExposureTrendQuery, Result<IReadOnlyList<FmcsaExposureSnapshotDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaExposureSnapshotDto>>> Handle(GetFmcsaExposureTrendQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var trend = await repository.GetExposureTrendAsync(
                request.TenantId, request.SiteId, request.Module, request.FromUtc, request.ToUtc, cancellationToken);
            return Result<IReadOnlyList<FmcsaExposureSnapshotDto>>.Success(trend);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA exposure trend for Module {Module} Tenant {TenantId}", request.Module, request.TenantId);

            return Result<IReadOnlyList<FmcsaExposureSnapshotDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.ExposureTrendRetrievalFailed",
                "Unable to retrieve the FMCSA exposure trend at this time.",
                Guid.NewGuid()));
        }
    }
}
