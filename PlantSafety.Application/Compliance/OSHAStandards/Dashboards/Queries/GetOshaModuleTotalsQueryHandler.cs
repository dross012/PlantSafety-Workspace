// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaModuleTotalsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Queries;

/// <summary>Handles <see cref="GetOshaModuleTotalsQuery"/>.</summary>
public sealed class GetOshaModuleTotalsQueryHandler(
    IOshaAuditGapRepository repository,
    ILogger<GetOshaModuleTotalsQueryHandler> logger)
    : IRequestHandler<GetOshaModuleTotalsQuery, Result<IReadOnlyList<OshaModuleTotalDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OshaModuleTotalDto>>> Handle(GetOshaModuleTotalsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totals = await repository.GetModuleTotalsAsync(cancellationToken);
            return Result<IReadOnlyList<OshaModuleTotalDto>>.Success(totals);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA module totals");

            return Result<IReadOnlyList<OshaModuleTotalDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.ModuleTotalsRetrievalFailed",
                "Unable to retrieve OSHA module totals at this time.",
                Guid.NewGuid()));
        }
    }
}
