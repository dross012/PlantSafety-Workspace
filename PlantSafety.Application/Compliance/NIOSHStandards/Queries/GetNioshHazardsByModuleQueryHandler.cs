// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNioshHazardsByModuleQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Handles <see cref="GetNioshHazardsByModuleQuery"/>.</summary>
public sealed class GetNioshHazardsByModuleQueryHandler(
    INioshReferenceRepository repository,
    ILogger<GetNioshHazardsByModuleQueryHandler> logger)
    : IRequestHandler<GetNioshHazardsByModuleQuery, Result<IReadOnlyList<NioshHazardProfileDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NioshHazardProfileDto>>> Handle(GetNioshHazardsByModuleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetHazardsByModuleAsync(request.Module, cancellationToken);
            return Result<IReadOnlyList<NioshHazardProfileDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NIOSH hazard profiles for module {Module}", request.Module);

            return Result<IReadOnlyList<NioshHazardProfileDto>>.Failure(new ApplicationError(
                "Compliance.NIOSHStandards.HazardsByModuleFailed",
                "Unable to retrieve NIOSH hazard profiles at this time.",
                Guid.NewGuid()));
        }
    }
}
