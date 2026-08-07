// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNioshHazardProfileQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Handles <see cref="GetNioshHazardProfileQuery"/>.</summary>
public sealed class GetNioshHazardProfileQueryHandler(
    INioshReferenceRepository repository,
    ILogger<GetNioshHazardProfileQueryHandler> logger)
    : IRequestHandler<GetNioshHazardProfileQuery, Result<NioshHazardProfileDto>>
{
    /// <inheritdoc/>
    public async Task<Result<NioshHazardProfileDto>> Handle(GetNioshHazardProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await repository.GetHazardProfileByCasAsync(request.CasNumber, cancellationToken);

            return dto is null
                ? Result<NioshHazardProfileDto>.Failure(new ApplicationError(
                    "Compliance.NIOSHStandards.NotFound",
                    "The requested NIOSH hazard profile was not found.",
                    Guid.NewGuid()))
                : Result<NioshHazardProfileDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NIOSH hazard profile for CAS {CasNumber}", request.CasNumber);

            return Result<NioshHazardProfileDto>.Failure(new ApplicationError(
                "Compliance.NIOSHStandards.RetrievalFailed",
                "Unable to retrieve the NIOSH hazard profile at this time.",
                Guid.NewGuid()));
        }
    }
}
