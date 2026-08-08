// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for ListEquipmentRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;
using PlantSafety.Application.Compliance.Standards.Repositories;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Handles <see cref="ListEquipmentRequirementsQuery"/>.</summary>
public sealed class ListEquipmentRequirementsQueryHandler(
    IStandardsReferenceRepository repository,
    ILogger<ListEquipmentRequirementsQueryHandler> logger)
    : IRequestHandler<ListEquipmentRequirementsQuery, Result<IReadOnlyList<EquipmentRequirementDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EquipmentRequirementDto>>> Handle(ListEquipmentRequirementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetEquipmentRequirementsAsync(request.EquipmentType, request.RequirementId, cancellationToken);
            return Result<IReadOnlyList<EquipmentRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list standards equipment requirements (EquipmentType={EquipmentType}, RequirementId={RequirementId})",
                request.EquipmentType, request.RequirementId);

            return Result<IReadOnlyList<EquipmentRequirementDto>>.Failure(new ApplicationError(
                "Compliance.Standards.EquipmentRequirementsFailed",
                "Unable to retrieve consensus-standards equipment requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
