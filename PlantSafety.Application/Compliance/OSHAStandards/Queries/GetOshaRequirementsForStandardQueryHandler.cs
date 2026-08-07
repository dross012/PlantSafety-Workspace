// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaRequirementsForStandardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Handles <see cref="GetOshaRequirementsForStandardQuery"/>.</summary>
public sealed class GetOshaRequirementsForStandardQueryHandler(
    IOSHAStandardsRepository repository,
    ILogger<GetOshaRequirementsForStandardQueryHandler> logger)
    : IRequestHandler<GetOshaRequirementsForStandardQuery, Result<IReadOnlyList<OSHARequirementItemDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OSHARequirementItemDto>>> Handle(GetOshaRequirementsForStandardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetRequirementsForStandardAsync(request.OSHAStandardId, cancellationToken);
            return Result<IReadOnlyList<OSHARequirementItemDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve requirements for OSHA standard {OSHAStandardId}", request.OSHAStandardId);

            return Result<IReadOnlyList<OSHARequirementItemDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.RequirementsRetrievalFailed",
                "Unable to retrieve OSHA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
