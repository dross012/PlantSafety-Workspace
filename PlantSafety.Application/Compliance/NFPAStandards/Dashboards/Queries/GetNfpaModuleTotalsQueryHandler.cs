// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaModuleTotalsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetNfpaModuleTotalsQuery"/>.</summary>
public sealed class GetNfpaModuleTotalsQueryHandler(
    INfpaAuditGapRepository repository,
    ILogger<GetNfpaModuleTotalsQueryHandler> logger)
    : IRequestHandler<GetNfpaModuleTotalsQuery, Result<IReadOnlyList<NfpaModuleTotalDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaModuleTotalDto>>> Handle(GetNfpaModuleTotalsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totals = await repository.GetModuleTotalsAsync(cancellationToken);
            return Result<IReadOnlyList<NfpaModuleTotalDto>>.Success(totals);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA module totals");

            return Result<IReadOnlyList<NfpaModuleTotalDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.ModuleTotalsRetrievalFailed",
                "Unable to retrieve NFPA module totals at this time.",
                Guid.NewGuid()));
        }
    }
}
