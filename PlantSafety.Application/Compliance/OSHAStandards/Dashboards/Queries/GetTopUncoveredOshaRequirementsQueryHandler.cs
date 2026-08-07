// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetTopUncoveredOshaRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetTopUncoveredOshaRequirementsQuery"/>.</summary>
public sealed class GetTopUncoveredOshaRequirementsQueryHandler(
    IOshaAuditGapRepository repository,
    ILogger<GetTopUncoveredOshaRequirementsQueryHandler> logger)
    : IRequestHandler<GetTopUncoveredOshaRequirementsQuery, Result<IReadOnlyList<OshaGapDto>>>
{
    private const int MaxTake = 100;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OshaGapDto>>> Handle(GetTopUncoveredOshaRequirementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? 10 : request.Take;
            var gaps = await repository.GetTopUncoveredForSiteAsync(request.TenantId, request.SiteId, take, cancellationToken);
            return Result<IReadOnlyList<OshaGapDto>>.Success(gaps);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve top uncovered OSHA requirements for Site {SiteId} Tenant {TenantId}", request.SiteId, request.TenantId);

            return Result<IReadOnlyList<OshaGapDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.TopUncoveredRetrievalFailed",
                "Unable to retrieve top uncovered OSHA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
