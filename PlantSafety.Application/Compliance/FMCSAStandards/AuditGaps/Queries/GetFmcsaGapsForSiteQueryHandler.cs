// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaGapsForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Queries;

/// <summary>Handles <see cref="GetFmcsaGapsForSiteQuery"/>.</summary>
public sealed class GetFmcsaGapsForSiteQueryHandler(
    IFmcsaAuditGapRepository repository,
    ILogger<GetFmcsaGapsForSiteQueryHandler> logger)
    : IRequestHandler<GetFmcsaGapsForSiteQuery, Result<IReadOnlyList<FmcsaGapDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaGapDto>>> Handle(GetFmcsaGapsForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var gaps = await repository.GetGapsForSiteAsync(request.TenantId, request.SiteId, request.Module, cancellationToken);
            return Result<IReadOnlyList<FmcsaGapDto>>.Success(gaps);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA gaps for Site {SiteId} Module {Module} Tenant {TenantId}",
                request.SiteId, request.Module, request.TenantId);

            return Result<IReadOnlyList<FmcsaGapDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.AuditGapsRetrievalFailed",
                "Unable to retrieve FMCSA audit gaps at this time.",
                Guid.NewGuid()));
        }
    }
}
