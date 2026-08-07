// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetRespiratorRecommendationsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Handles <see cref="GetRespiratorRecommendationsQuery"/>.</summary>
public sealed class GetRespiratorRecommendationsQueryHandler(
    INioshReferenceRepository repository,
    ILogger<GetRespiratorRecommendationsQueryHandler> logger)
    : IRequestHandler<GetRespiratorRecommendationsQuery, Result<IReadOnlyList<NioshRespiratorMatrixDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NioshRespiratorMatrixDto>>> Handle(GetRespiratorRecommendationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetRespiratorRecommendationsAsync(request.HazardId, cancellationToken);
            return Result<IReadOnlyList<NioshRespiratorMatrixDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NIOSH respirator recommendations for hazard {HazardId}", request.HazardId);

            return Result<IReadOnlyList<NioshRespiratorMatrixDto>>.Failure(new ApplicationError(
                "Compliance.NIOSHStandards.RespiratorRecommendationsFailed",
                "Unable to retrieve NIOSH respirator recommendations at this time.",
                Guid.NewGuid()));
        }
    }
}
