// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for ListEpaReportingRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Handles <see cref="ListEpaReportingRequirementsQuery"/>.</summary>
public sealed class ListEpaReportingRequirementsQueryHandler(
    IEpaReferenceRepository repository,
    ILogger<ListEpaReportingRequirementsQueryHandler> logger)
    : IRequestHandler<ListEpaReportingRequirementsQuery, Result<IReadOnlyList<EpaReportingRequirementDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EpaReportingRequirementDto>>> Handle(ListEpaReportingRequirementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetReportingRequirementsAsync(request.ProgramCode, cancellationToken);
            return Result<IReadOnlyList<EpaReportingRequirementDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list EPA reporting requirements (ProgramCode={ProgramCode})", request.ProgramCode);

            return Result<IReadOnlyList<EpaReportingRequirementDto>>.Failure(new ApplicationError(
                "Compliance.EPA.ReportingRequirementsFailed",
                "Unable to retrieve EPA reporting requirements at this time.",
                Guid.NewGuid()));
        }
    }
}
