// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for SearchOshaRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Handles <see cref="SearchOshaRequirementsQuery"/>.</summary>
public sealed class SearchOshaRequirementsQueryHandler(
    IOSHAStandardsRepository repository,
    ILogger<SearchOshaRequirementsQueryHandler> logger)
    : IRequestHandler<SearchOshaRequirementsQuery, Result<IReadOnlyList<OSHARequirementItemDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OSHARequirementItemDto>>> Handle(SearchOshaRequirementsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
        {
            return Result<IReadOnlyList<OSHARequirementItemDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.KeywordRequired",
                "A search keyword is required.",
                Guid.NewGuid()));
        }

        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.SearchRequirementsAsync(request.Keyword, skip, take, cancellationToken);
            return Result<IReadOnlyList<OSHARequirementItemDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search OSHA requirements (Keyword={Keyword})", request.Keyword);

            return Result<IReadOnlyList<OSHARequirementItemDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.RequirementSearchFailed",
                "Unable to search OSHA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
