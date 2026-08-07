// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaHazardModulesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Handles <see cref="GetNfpaHazardModulesQuery"/>.</summary>
public sealed class GetNfpaHazardModulesQueryHandler(
    INfpaReferenceRepository repository,
    ILogger<GetNfpaHazardModulesQueryHandler> logger)
    : IRequestHandler<GetNfpaHazardModulesQuery, Result<IReadOnlyList<NfpaHazardModuleDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaHazardModuleDto>>> Handle(GetNfpaHazardModulesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetHazardsByModuleAsync(request.ModuleName, cancellationToken);
            return Result<IReadOnlyList<NfpaHazardModuleDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA hazards for module {ModuleName}", request.ModuleName);

            return Result<IReadOnlyList<NfpaHazardModuleDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.HazardsByModuleFailed",
                "Unable to retrieve NFPA hazards at this time.",
                Guid.NewGuid()));
        }
    }
}
