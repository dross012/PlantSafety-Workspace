// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for ListEpaRecordkeepingRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Handles <see cref="ListEpaRecordkeepingRequirementsQuery"/>.</summary>
public sealed class ListEpaRecordkeepingRequirementsQueryHandler(
    IEpaReferenceRepository repository,
    ILogger<ListEpaRecordkeepingRequirementsQueryHandler> logger)
    : IRequestHandler<ListEpaRecordkeepingRequirementsQuery, Result<IReadOnlyList<EpaRecordkeepingRequirementDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EpaRecordkeepingRequirementDto>>> Handle(ListEpaRecordkeepingRequirementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetRecordkeepingRequirementsAsync(request.ProgramCode, cancellationToken);
            return Result<IReadOnlyList<EpaRecordkeepingRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list EPA recordkeeping requirements (ProgramCode={ProgramCode})", request.ProgramCode);

            return Result<IReadOnlyList<EpaRecordkeepingRequirementDto>>.Failure(new ApplicationError(
                "Compliance.EPA.RecordkeepingRequirementsFailed",
                "Unable to retrieve EPA recordkeeping requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
