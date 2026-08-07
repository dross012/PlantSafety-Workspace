// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaGapsForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Queries;

/// <summary>Handles <see cref="GetOshaGapsForSiteQuery"/>.</summary>
public sealed class GetOshaGapsForSiteQueryHandler(
    IOshaAuditGapRepository repository,
    ILogger<GetOshaGapsForSiteQueryHandler> logger)
    : IRequestHandler<GetOshaGapsForSiteQuery, Result<IReadOnlyList<OshaGapDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OshaGapDto>>> Handle(GetOshaGapsForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var gaps = await repository.GetGapsForSiteAsync(request.TenantId, request.SiteId, request.Module, cancellationToken);
            return Result<IReadOnlyList<OshaGapDto>>.Success(gaps);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA gaps for Site {SiteId} Module {Module} Tenant {TenantId}",
                request.SiteId, request.Module, request.TenantId);

            return Result<IReadOnlyList<OshaGapDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.AuditGapsRetrievalFailed",
                "Unable to retrieve OSHA audit gaps at this time.",
                Guid.NewGuid()));
        }
    }
}
