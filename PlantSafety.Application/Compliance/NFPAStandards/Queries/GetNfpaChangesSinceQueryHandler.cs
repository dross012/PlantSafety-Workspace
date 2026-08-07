// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaChangesSinceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Handles <see cref="GetNfpaChangesSinceQuery"/>.</summary>
public sealed class GetNfpaChangesSinceQueryHandler(
    INfpaReferenceRepository repository,
    ILogger<GetNfpaChangesSinceQueryHandler> logger)
    : IRequestHandler<GetNfpaChangesSinceQuery, Result<IReadOnlyList<NfpaHazardChangeDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaHazardChangeDto>>> Handle(GetNfpaChangesSinceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetChangesSinceAsync(request.SinceUtc, cancellationToken);
            return Result<IReadOnlyList<NfpaHazardChangeDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA hazard changes since {SinceUtc}", request.SinceUtc);

            return Result<IReadOnlyList<NfpaHazardChangeDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.ChangesRetrievalFailed",
                "Unable to retrieve NFPA hazard changes at this time.",
                Guid.NewGuid()));
        }
    }
}
