// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetStandardsFamiliesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;
using PlantSafety.Application.Compliance.Standards.Repositories;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Handles <see cref="GetStandardsFamiliesQuery"/>.</summary>
public sealed class GetStandardsFamiliesQueryHandler(
    IStandardsReferenceRepository repository,
    ILogger<GetStandardsFamiliesQueryHandler> logger)
    : IRequestHandler<GetStandardsFamiliesQuery, Result<IReadOnlyList<StandardsFamilyDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<StandardsFamilyDto>>> Handle(GetStandardsFamiliesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetAllFamiliesAsync(cancellationToken);
            return Result<IReadOnlyList<StandardsFamilyDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve standards families");

            return Result<IReadOnlyList<StandardsFamilyDto>>.Failure(new ApplicationError(
                "Compliance.Standards.FamiliesFailed",
                "Unable to retrieve consensus-standards families at this time.",
                Guid.NewGuid()));
        }
    }
}
