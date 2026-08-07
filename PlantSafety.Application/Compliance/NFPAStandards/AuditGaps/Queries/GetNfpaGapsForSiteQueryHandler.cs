// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaGapsForSiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Queries;

/// <summary>Handles <see cref="GetNfpaGapsForSiteQuery"/>.</summary>
public sealed class GetNfpaGapsForSiteQueryHandler(
    INfpaAuditGapRepository repository,
    ILogger<GetNfpaGapsForSiteQueryHandler> logger)
    : IRequestHandler<GetNfpaGapsForSiteQuery, Result<IReadOnlyList<NfpaGapDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaGapDto>>> Handle(GetNfpaGapsForSiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var gaps = await repository.GetGapsForSiteAsync(request.TenantId, request.SiteId, request.Module, cancellationToken);
            return Result<IReadOnlyList<NfpaGapDto>>.Success(gaps);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA gaps for Site {SiteId} Module {Module} Tenant {TenantId}",
                request.SiteId, request.Module, request.TenantId);

            return Result<IReadOnlyList<NfpaGapDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.AuditGapsRetrievalFailed",
                "Unable to retrieve NFPA audit gaps at this time.",
                Guid.NewGuid()));
        }
    }
}
