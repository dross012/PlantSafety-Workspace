// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for SearchNfpaHazardsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Handles <see cref="SearchNfpaHazardsQuery"/>.</summary>
public sealed class SearchNfpaHazardsQueryHandler(
    INfpaReferenceRepository repository,
    ILogger<SearchNfpaHazardsQueryHandler> logger)
    : IRequestHandler<SearchNfpaHazardsQuery, Result<IReadOnlyList<NfpaHazardProfileDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NfpaHazardProfileDto>>> Handle(SearchNfpaHazardsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.SearchHazardsAsync(request.Keyword, request.CodeNumber, skip, take, cancellationToken);
            return Result<IReadOnlyList<NfpaHazardProfileDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search NFPA hazards (Keyword={Keyword}, CodeNumber={CodeNumber})",
                request.Keyword, request.CodeNumber);

            return Result<IReadOnlyList<NfpaHazardProfileDto>>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.SearchFailed",
                "Unable to search NFPA hazards at this time.",
                Guid.NewGuid()));
        }
    }
}
