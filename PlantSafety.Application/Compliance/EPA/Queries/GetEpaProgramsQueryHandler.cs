// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetEpaProgramsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Handles <see cref="GetEpaProgramsQuery"/>.</summary>
public sealed class GetEpaProgramsQueryHandler(
    IEpaReferenceRepository repository,
    ILogger<GetEpaProgramsQueryHandler> logger)
    : IRequestHandler<GetEpaProgramsQuery, Result<IReadOnlyList<EpaProgramDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EpaProgramDto>>> Handle(GetEpaProgramsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetAllProgramsAsync(cancellationToken);
            return Result<IReadOnlyList<EpaProgramDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve EPA programs");

            return Result<IReadOnlyList<EpaProgramDto>>.Failure(new ApplicationError(
                "Compliance.EPA.ProgramsFailed",
                "Unable to retrieve EPA programs at this time.",
                Guid.NewGuid()));
        }
    }
}
