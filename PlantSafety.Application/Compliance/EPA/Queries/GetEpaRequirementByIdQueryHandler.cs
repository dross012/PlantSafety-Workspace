// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetEpaRequirementByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Handles <see cref="GetEpaRequirementByIdQuery"/>.</summary>
public sealed class GetEpaRequirementByIdQueryHandler(
    IEpaReferenceRepository repository,
    ILogger<GetEpaRequirementByIdQueryHandler> logger)
    : IRequestHandler<GetEpaRequirementByIdQuery, Result<EpaRegulatoryRequirementDto>>
{
    /// <inheritdoc/>
    public async Task<Result<EpaRegulatoryRequirementDto>> Handle(GetEpaRequirementByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await repository.GetRequirementByIdAsync(request.RequirementId, cancellationToken);

            return dto is null
                ? Result<EpaRegulatoryRequirementDto>.Failure(new ApplicationError(
                    "Compliance.EPA.NotFound",
                    "The requested EPA requirement was not found.",
                    Guid.NewGuid()))
                : Result<EpaRegulatoryRequirementDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve EPA requirement {RequirementId}", request.RequirementId);

            return Result<EpaRegulatoryRequirementDto>.Failure(new ApplicationError(
                "Compliance.EPA.RetrievalFailed",
                "Unable to retrieve the EPA requirement at this time.",
                Guid.NewGuid()));
        }
    }
}
