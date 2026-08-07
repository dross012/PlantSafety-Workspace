// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetOshaStandardBySectionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Repositories;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Handles <see cref="GetOshaStandardBySectionQuery"/>.</summary>
public sealed class GetOshaStandardBySectionQueryHandler(
    IOSHAStandardsRepository repository,
    ILogger<GetOshaStandardBySectionQueryHandler> logger)
    : IRequestHandler<GetOshaStandardBySectionQuery, Result<OSHAStandardItemDto>>
{
    /// <inheritdoc/>
    public async Task<Result<OSHAStandardItemDto>> Handle(GetOshaStandardBySectionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await repository.GetBySectionAsync(request.PartNumber, request.Section, cancellationToken);

            return dto is null
                ? Result<OSHAStandardItemDto>.Failure(new ApplicationError(
                    "Compliance.OSHAStandards.NotFound",
                    "The requested OSHA standard was not found.",
                    Guid.NewGuid()))
                : Result<OSHAStandardItemDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve OSHA standard Part {PartNumber} Section {Section}", request.PartNumber, request.Section);

            return Result<OSHAStandardItemDto>.Failure(new ApplicationError(
                "Compliance.OSHAStandards.RetrievalFailed",
                "Unable to retrieve the OSHA standard at this time.",
                Guid.NewGuid()));
        }
    }
}
