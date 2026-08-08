// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for ListEpaThresholdRulesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Handles <see cref="ListEpaThresholdRulesQuery"/>.</summary>
public sealed class ListEpaThresholdRulesQueryHandler(
    IEpaReferenceRepository repository,
    ILogger<ListEpaThresholdRulesQueryHandler> logger)
    : IRequestHandler<ListEpaThresholdRulesQuery, Result<IReadOnlyList<EpaThresholdRuleDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EpaThresholdRuleDto>>> Handle(ListEpaThresholdRulesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetThresholdRulesAsync(request.ProgramCode, request.ParameterName, cancellationToken);
            return Result<IReadOnlyList<EpaThresholdRuleDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list EPA threshold rules (ProgramCode={ProgramCode}, ParameterName={ParameterName})",
                request.ProgramCode, request.ParameterName);

            return Result<IReadOnlyList<EpaThresholdRuleDto>>.Failure(new ApplicationError(
                "Compliance.EPA.ThresholdRulesFailed",
                "Unable to retrieve EPA threshold rules at this time.",
                Guid.NewGuid()));
        }
    }
}
