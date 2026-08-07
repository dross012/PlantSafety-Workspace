// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaExposureTrendQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetOshaExposureTrendQuery"/>.</summary>
public sealed class GetOshaExposureTrendQueryHandler(
    IOshaAuditGapRepository repository,
    ILogger<GetOshaExposureTrendQueryHandler> logger)
    : IRequestHandler<GetOshaExposureTrendQuery, Result<IReadOnlyList<OshaExposureSnapshotDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OshaExposureSnapshotDto>>> Handle(GetOshaExposureTrendQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var trend = await repository.GetExposureTrendAsync(
                request.TenantId, request.SiteId, request.Module, request.FromUtc, request.ToUtc, cancellationToken);
            return Result<IReadOnlyList<OshaExposureSnapshotDto>>.Success(trend);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA exposure trend for Module {Module} Tenant {TenantId}", request.Module, request.TenantId);

            return Result<IReadOnlyList<OshaExposureSnapshotDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.ExposureTrendRetrievalFailed",
                "Unable to retrieve the OSHA exposure trend at this time.",
                Guid.NewGuid()));
        }
    }
}
