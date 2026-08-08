// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetEpaRequirementsByModuleQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Handles <see cref="GetEpaRequirementsByModuleQuery"/>.</summary>
public sealed class GetEpaRequirementsByModuleQueryHandler(
    IEpaReferenceRepository repository,
    ILogger<GetEpaRequirementsByModuleQueryHandler> logger)
    : IRequestHandler<GetEpaRequirementsByModuleQuery, Result<IReadOnlyList<EpaRegulatoryRequirementDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EpaRegulatoryRequirementDto>>> Handle(GetEpaRequirementsByModuleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.GetRequirementsByModuleAsync(request.ModuleName, skip, take, cancellationToken);
            return Result<IReadOnlyList<EpaRegulatoryRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve EPA requirements for module {ModuleName}", request.ModuleName);

            return Result<IReadOnlyList<EpaRegulatoryRequirementDto>>.Failure(new ApplicationError(
                "Compliance.EPA.RequirementsByModuleFailed",
                "Unable to retrieve EPA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
