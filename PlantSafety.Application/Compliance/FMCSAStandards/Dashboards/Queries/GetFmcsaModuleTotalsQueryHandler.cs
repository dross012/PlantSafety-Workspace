// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaModuleTotalsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetFmcsaModuleTotalsQuery"/>.</summary>
public sealed class GetFmcsaModuleTotalsQueryHandler(
    IFmcsaAuditGapRepository repository,
    ILogger<GetFmcsaModuleTotalsQueryHandler> logger)
    : IRequestHandler<GetFmcsaModuleTotalsQuery, Result<IReadOnlyList<FmcsaModuleTotalDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaModuleTotalDto>>> Handle(GetFmcsaModuleTotalsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totals = await repository.GetModuleTotalsAsync(cancellationToken);
            return Result<IReadOnlyList<FmcsaModuleTotalDto>>.Success(totals);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA module totals");

            return Result<IReadOnlyList<FmcsaModuleTotalDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.ModuleTotalsRetrievalFailed",
                "Unable to retrieve FMCSA module totals at this time.",
                Guid.NewGuid()));
        }
    }
}
