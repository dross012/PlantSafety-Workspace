// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetPpeGuidanceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Handles <see cref="GetPpeGuidanceQuery"/>.</summary>
public sealed class GetPpeGuidanceQueryHandler(
    INioshReferenceRepository repository,
    ILogger<GetPpeGuidanceQueryHandler> logger)
    : IRequestHandler<GetPpeGuidanceQuery, Result<IReadOnlyList<NioshPpeGuidanceDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NioshPpeGuidanceDto>>> Handle(GetPpeGuidanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetPpeGuidanceAsync(request.HazardId, cancellationToken);
            return Result<IReadOnlyList<NioshPpeGuidanceDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NIOSH PPE guidance for hazard {HazardId}", request.HazardId);

            return Result<IReadOnlyList<NioshPpeGuidanceDto>>.Failure(new ApplicationError(
                "Compliance.NIOSHStandards.PpeGuidanceFailed",
                "Unable to retrieve NIOSH PPE guidance at this time.",
                Guid.NewGuid()));
        }
    }
}
