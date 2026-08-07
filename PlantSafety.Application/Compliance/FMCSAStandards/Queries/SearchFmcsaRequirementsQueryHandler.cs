// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for SearchFmcsaRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Handles <see cref="SearchFmcsaRequirementsQuery"/>.</summary>
public sealed class SearchFmcsaRequirementsQueryHandler(
    IFmcsaStandardsRepository repository,
    ILogger<SearchFmcsaRequirementsQueryHandler> logger)
    : IRequestHandler<SearchFmcsaRequirementsQuery, Result<IReadOnlyList<FmcsaRequirementDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaRequirementDto>>> Handle(SearchFmcsaRequirementsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
        {
            return Result<IReadOnlyList<FmcsaRequirementDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.KeywordRequired",
                "A search keyword is required.",
                Guid.NewGuid()));
        }

        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.SearchRequirementsAsync(request.Keyword, skip, take, cancellationToken);
            return Result<IReadOnlyList<FmcsaRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search FMCSA requirements (Keyword={Keyword})", request.Keyword);

            return Result<IReadOnlyList<FmcsaRequirementDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.RequirementSearchFailed",
                "Unable to search FMCSA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
