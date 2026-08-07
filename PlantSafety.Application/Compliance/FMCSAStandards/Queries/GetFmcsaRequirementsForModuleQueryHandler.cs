// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaRequirementsForModuleQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Handles <see cref="GetFmcsaRequirementsForModuleQuery"/>.</summary>
public sealed class GetFmcsaRequirementsForModuleQueryHandler(
    IFmcsaStandardsRepository repository,
    ILogger<GetFmcsaRequirementsForModuleQueryHandler> logger)
    : IRequestHandler<GetFmcsaRequirementsForModuleQuery, Result<IReadOnlyList<FmcsaRequirementDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaRequirementDto>>> Handle(GetFmcsaRequirementsForModuleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetRequirementsByModuleAsync(request.AppliesToModule, cancellationToken);
            return Result<IReadOnlyList<FmcsaRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA requirements for module {AppliesToModule}", request.AppliesToModule);

            return Result<IReadOnlyList<FmcsaRequirementDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.RequirementsByModuleFailed",
                "Unable to retrieve FMCSA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
