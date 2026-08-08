// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for ListRiskAssessmentRulesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;
using PlantSafety.Application.Compliance.Standards.Repositories;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Handles <see cref="ListRiskAssessmentRulesQuery"/>.</summary>
public sealed class ListRiskAssessmentRulesQueryHandler(
    IStandardsReferenceRepository repository,
    ILogger<ListRiskAssessmentRulesQueryHandler> logger)
    : IRequestHandler<ListRiskAssessmentRulesQuery, Result<IReadOnlyList<RiskAssessmentRuleDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<RiskAssessmentRuleDto>>> Handle(ListRiskAssessmentRulesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await repository.GetRiskAssessmentRulesAsync(request.Methodology, request.RequirementId, cancellationToken);
            return Result<IReadOnlyList<RiskAssessmentRuleDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list standards risk assessment rules (Methodology={Methodology}, RequirementId={RequirementId})",
                request.Methodology, request.RequirementId);

            return Result<IReadOnlyList<RiskAssessmentRuleDto>>.Failure(new ApplicationError(
                "Compliance.Standards.RiskAssessmentRulesFailed",
                "Unable to retrieve consensus-standards risk assessment rules at this time.",
                Guid.NewGuid()));
        }
    }
}
