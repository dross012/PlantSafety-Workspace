// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for SearchOshaStandardsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Handles <see cref="SearchOshaStandardsQuery"/>.</summary>
public sealed class SearchOshaStandardsQueryHandler(
    IOSHAStandardsRepository repository,
    ILogger<SearchOshaStandardsQueryHandler> logger)
    : IRequestHandler<SearchOshaStandardsQuery, Result<IReadOnlyList<OSHAStandardItemDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OSHAStandardItemDto>>> Handle(SearchOshaStandardsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.SearchAsync(request.Keyword, request.PartNumber, request.Status, skip, take, cancellationToken);
            return Result<IReadOnlyList<OSHAStandardItemDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search OSHA standards (Keyword={Keyword}, PartNumber={PartNumber}, Status={Status})",
                request.Keyword, request.PartNumber, request.Status);

            return Result<IReadOnlyList<OSHAStandardItemDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.SearchFailed",
                "Unable to search OSHA standards at this time.",
                Guid.NewGuid()));
        }
    }
}
