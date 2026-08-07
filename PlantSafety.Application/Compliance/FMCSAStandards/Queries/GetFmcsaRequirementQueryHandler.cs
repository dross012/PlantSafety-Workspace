// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaRequirementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Handles <see cref="GetFmcsaRequirementQuery"/>.</summary>
public sealed class GetFmcsaRequirementQueryHandler(
    IFmcsaStandardsRepository repository,
    ILogger<GetFmcsaRequirementQueryHandler> logger)
    : IRequestHandler<GetFmcsaRequirementQuery, Result<IReadOnlyList<FmcsaRequirementDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaRequirementDto>>> Handle(GetFmcsaRequirementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetRequirementsForStandardAsync(request.StandardId, cancellationToken);
            return Result<IReadOnlyList<FmcsaRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve requirements for FMCSA standard {StandardId}", request.StandardId);

            return Result<IReadOnlyList<FmcsaRequirementDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.RequirementsRetrievalFailed",
                "Unable to retrieve FMCSA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
