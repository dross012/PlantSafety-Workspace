// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: FluentValidation validators for GetComplianceEvaluationQuery and
//   GetComplianceEvaluationHistoryQuery.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Compliance.RulesEngine.Validation;

/// <summary>Validates <see cref="Queries.GetComplianceEvaluationQuery"/>.</summary>
public sealed class GetComplianceEvaluationQueryValidator : AbstractValidator<Queries.GetComplianceEvaluationQuery>
{
    /// <summary>Defines validation rules for <see cref="Queries.GetComplianceEvaluationQuery"/>.</summary>
    public GetComplianceEvaluationQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ComplianceEvaluationId).NotEmpty();
    }
}

/// <summary>Validates <see cref="Queries.GetComplianceEvaluationHistoryQuery"/>.</summary>
public sealed class GetComplianceEvaluationHistoryQueryValidator : AbstractValidator<Queries.GetComplianceEvaluationHistoryQuery>
{
    /// <summary>Defines validation rules for <see cref="Queries.GetComplianceEvaluationHistoryQuery"/>.</summary>
    public GetComplianceEvaluationHistoryQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TargetEntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetEntityId).NotEmpty();
    }
}
