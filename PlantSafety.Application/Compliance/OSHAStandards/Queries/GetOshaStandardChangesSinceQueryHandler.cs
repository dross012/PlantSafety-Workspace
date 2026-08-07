// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaStandardChangesSinceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Handles <see cref="GetOshaStandardChangesSinceQuery"/>.</summary>
public sealed class GetOshaStandardChangesSinceQueryHandler(
    IOSHAStandardsRepository repository,
    ILogger<GetOshaStandardChangesSinceQueryHandler> logger)
    : IRequestHandler<GetOshaStandardChangesSinceQuery, Result<IReadOnlyList<OSHAStandardChangeDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OSHAStandardChangeDto>>> Handle(GetOshaStandardChangesSinceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetChangesSinceAsync(request.SinceUtc, cancellationToken);
            return Result<IReadOnlyList<OSHAStandardChangeDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA standard changes since {SinceUtc}", request.SinceUtc);

            return Result<IReadOnlyList<OSHAStandardChangeDto>>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.ChangesRetrievalFailed",
                "Unable to retrieve OSHA standard changes at this time.",
                Guid.NewGuid()));
        }
    }
}
