// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaChangesSinceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Handles <see cref="GetFmcsaChangesSinceQuery"/>.</summary>
public sealed class GetFmcsaChangesSinceQueryHandler(
    IFmcsaStandardsRepository repository,
    ILogger<GetFmcsaChangesSinceQueryHandler> logger)
    : IRequestHandler<GetFmcsaChangesSinceQuery, Result<IReadOnlyList<FmcsaStandardChangeDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FmcsaStandardChangeDto>>> Handle(GetFmcsaChangesSinceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetChangesSinceAsync(request.SinceUtc, cancellationToken);
            return Result<IReadOnlyList<FmcsaStandardChangeDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA standard changes since {SinceUtc}", request.SinceUtc);

            return Result<IReadOnlyList<FmcsaStandardChangeDto>>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.ChangesRetrievalFailed",
                "Unable to retrieve FMCSA standard changes at this time.",
                Guid.NewGuid()));
        }
    }
}
