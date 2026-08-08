// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetStandardsByFamilyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;
using PlantSafety.Application.Compliance.Standards.Repositories;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Handles <see cref="GetStandardsByFamilyQuery"/>.</summary>
public sealed class GetStandardsByFamilyQueryHandler(
    IStandardsReferenceRepository repository,
    ILogger<GetStandardsByFamilyQueryHandler> logger)
    : IRequestHandler<GetStandardsByFamilyQuery, Result<IReadOnlyList<StandardsRequirementDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<StandardsRequirementDto>>> Handle(GetStandardsByFamilyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.GetRequirementsByFamilyAsync(request.FamilyCode, skip, take, cancellationToken);
            return Result<IReadOnlyList<StandardsRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve standards requirements for family {FamilyCode}", request.FamilyCode);

            return Result<IReadOnlyList<StandardsRequirementDto>>.Failure(new ApplicationError(
                "Compliance.Standards.RequirementsByFamilyFailed",
                "Unable to retrieve consensus-standards requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
