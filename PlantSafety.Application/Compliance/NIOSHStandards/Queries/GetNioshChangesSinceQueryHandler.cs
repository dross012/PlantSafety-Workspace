// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNioshChangesSinceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Handles <see cref="GetNioshChangesSinceQuery"/>.</summary>
public sealed class GetNioshChangesSinceQueryHandler(
    INioshReferenceRepository repository,
    ILogger<GetNioshChangesSinceQueryHandler> logger)
    : IRequestHandler<GetNioshChangesSinceQuery, Result<IReadOnlyList<NioshHazardChangeDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NioshHazardChangeDto>>> Handle(GetNioshChangesSinceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetChangesSinceAsync(request.SinceUtc, cancellationToken);
            return Result<IReadOnlyList<NioshHazardChangeDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NIOSH hazard changes since {SinceUtc}", request.SinceUtc);

            return Result<IReadOnlyList<NioshHazardChangeDto>>.Failure(new ApplicationError(
                "Compliance.NIOSHStandards.ChangesRetrievalFailed",
                "Unable to retrieve NIOSH hazard changes at this time.",
                Guid.NewGuid()));
        }
    }
}
