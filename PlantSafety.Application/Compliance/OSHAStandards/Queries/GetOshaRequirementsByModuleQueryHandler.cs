// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaRequirementsByModuleQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Handles <see cref="GetOshaRequirementsByModuleQuery"/>.</summary>
public sealed class GetOshaRequirementsByModuleQueryHandler(
    IOSHAStandardsRepository repository,
    ILogger<GetOshaRequirementsByModuleQueryHandler> logger)
    : IRequestHandler<GetOshaRequirementsByModuleQuery, Result<IReadOnlyList<OSHARequirementItemDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OSHARequirementItemDto>>> Handle(GetOshaRequirementsByModuleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetRequirementsByModuleAsync(request.AppliesToModule, cancellationToken);
            return Result<IReadOnlyList<OSHARequirementItemDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA requirements for module {AppliesToModule}", request.AppliesToModule);

            return Result<IReadOnlyList<OSHARequirementItemDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.RequirementsByModuleFailed",
                "Unable to retrieve OSHA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
