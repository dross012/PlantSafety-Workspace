// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for SearchFmcsaStandardsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Handles <see cref="SearchFmcsaStandardsQuery"/>.</summary>
public sealed class SearchFmcsaStandardsQueryHandler(
    IFmcsaStandardsRepository repository,
    ILogger<SearchFmcsaStandardsQueryHandler> logger)
    : IRequestHandler<SearchFmcsaStandardsQuery, Result<IReadOnlyList<FmcsaStandardDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaStandardDto>>> Handle(SearchFmcsaStandardsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.SearchStandardsAsync(request.Keyword, request.PartNumber, skip, take, cancellationToken);
            return Result<IReadOnlyList<FmcsaStandardDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search FMCSA standards (Keyword={Keyword}, PartNumber={PartNumber})",
                request.Keyword, request.PartNumber);

            return Result<IReadOnlyList<FmcsaStandardDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.SearchFailed",
                "Unable to search FMCSA standards at this time.",
                Guid.NewGuid()));
        }
    }
}
