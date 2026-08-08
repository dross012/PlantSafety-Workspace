// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetStandardsRequirementByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;
using PlantSafety.Application.Compliance.Standards.Repositories;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Handles <see cref="GetStandardsRequirementByIdQuery"/>.</summary>
public sealed class GetStandardsRequirementByIdQueryHandler(
    IStandardsReferenceRepository repository,
    ILogger<GetStandardsRequirementByIdQueryHandler> logger)
    : IRequestHandler<GetStandardsRequirementByIdQuery, Result<StandardsRequirementDto>>
{
    /// <inheritdoc/>
    public async Task<Result<StandardsRequirementDto>> Handle(GetStandardsRequirementByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await repository.GetRequirementByIdAsync(request.RequirementId, cancellationToken);

            return dto is null
                ? Result<StandardsRequirementDto>.Failure(new ApplicationError(
                    "Compliance.Standards.NotFound",
                    "The requested consensus-standards requirement was not found.",
                    Guid.NewGuid()))
                : Result<StandardsRequirementDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve standards requirement {RequirementId}", request.RequirementId);

            return Result<StandardsRequirementDto>.Failure(new ApplicationError(
                "Compliance.Standards.RetrievalFailed",
                "Unable to retrieve the consensus-standards requirement at this time.",
                Guid.NewGuid()));
        }
    }
}
