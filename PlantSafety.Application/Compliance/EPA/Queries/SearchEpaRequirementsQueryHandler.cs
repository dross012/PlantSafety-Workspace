// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for SearchEpaRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Handles <see cref="SearchEpaRequirementsQuery"/>.</summary>
public sealed class SearchEpaRequirementsQueryHandler(
    IEpaReferenceRepository repository,
    ILogger<SearchEpaRequirementsQueryHandler> logger)
    : IRequestHandler<SearchEpaRequirementsQuery, Result<IReadOnlyList<EpaRegulatoryRequirementDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EpaRegulatoryRequirementDto>>> Handle(SearchEpaRequirementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.SearchRequirementsAsync(request.Keyword, request.ProgramCode, skip, take, cancellationToken);
            return Result<IReadOnlyList<EpaRegulatoryRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search EPA requirements (Keyword={Keyword}, ProgramCode={ProgramCode})",
                request.Keyword, request.ProgramCode);

            return Result<IReadOnlyList<EpaRegulatoryRequirementDto>>.Failure(new ApplicationError(
                "Compliance.EPA.SearchFailed",
                "Unable to search EPA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
