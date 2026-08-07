// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaControlsForHazardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Handles <see cref="GetNfpaControlsForHazardQuery"/>.</summary>
public sealed class GetNfpaControlsForHazardQueryHandler(
    INfpaReferenceRepository repository,
    ILogger<GetNfpaControlsForHazardQueryHandler> logger)
    : IRequestHandler<GetNfpaControlsForHazardQuery, Result<IReadOnlyList<NfpaControlMatrixDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaControlMatrixDto>>> Handle(GetNfpaControlsForHazardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetControlsForHazardAsync(request.HazardId, cancellationToken);
            return Result<IReadOnlyList<NfpaControlMatrixDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA controls for Hazard {HazardId}", request.HazardId);

            return Result<IReadOnlyList<NfpaControlMatrixDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.ControlsRetrievalFailed",
                "Unable to retrieve NFPA controls at this time.",
                Guid.NewGuid()));
        }
    }
}
