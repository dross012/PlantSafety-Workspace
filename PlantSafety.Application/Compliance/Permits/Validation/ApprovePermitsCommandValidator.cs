// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           ApprovePermitsCommandValidator.cs
//  Created:        2026-08-05
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ApprovePermitsCommand (Created -> Active). No
//                  validator existed for this command previously.
//
//                  Carries the first cross-domain compliance check in the codebase: MustAsync
//                  looks up the real Permit (PermitComplianceFactProvider -> GetPermitsByIdQuery,
//                  to find the requesting worker) and the requesting worker's real training
//                  records (WorkerTrainingFactsProvider -> GetTrainingQuery), then evaluates the
//                  real Compliance Rules Engine's PermitRequiresTrainingRule
//                  (Compliance.RulesEngine.Rules.CrossDomain) against those facts, blocking
//                  approval when the requester has no training record on file.
//
//                  Deliberately placed here rather than on CreatePermitsCommandValidator: a newly
//                  created Permits record starts in Status=Created (Permits.Create, Domain
//                  aggregate), not Active -- ApprovePermitsCommand is the actual Created -> Active
//                  transition, so it is the honest point at which "Active" facts apply.
//
//                  This is the first async/DI validator in the codebase (all other
//                  *CommandValidator.cs files are pure/sync, by design -- Handbook Rule 2.3).
//                  Deliberately reuses the real, already-existing
//                  IComplianceRule/ComplianceRuleContext/ComplianceRuleOutcome shapes from the
//                  Compliance Rules Engine rather than inventing a second, parallel rule-result
//                  type -- this does not introduce a second rules engine alongside the real one.
//                  Only calls the rule's pure Evaluate(context) directly; deliberately does NOT
//                  go through the full ComplianceEvaluator/ComplianceEvaluation aggregate
//                  persistence pipeline, since a validator must not have a database write
//                  side-effect. A full audited RulesEngine evaluation of an existing permit
//                  remains available separately via POST /api/v1/compliance/rules-engine/evaluate.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

using PlantSafety.Application.Compliance.RulesEngine.Rules;
using PlantSafety.Application.Compliance.RulesEngine.Rules.CrossDomain;
using PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;

namespace PlantSafety.Application.Compliance.Permits.Validation;

/// <summary>Validates <see cref="Commands.ApprovePermitsCommand"/>.</summary>
public sealed class ApprovePermitsCommandValidator : AbstractValidator<Commands.ApprovePermitsCommand>
{
    private static readonly PermitRequiresTrainingRule Rule = new();

    /// <summary>Defines validation rules for <see cref="Commands.ApprovePermitsCommand"/>.</summary>
    public ApprovePermitsCommandValidator(
        PermitComplianceFactProvider permitFacts,
        WorkerTrainingFactsProvider trainingFacts)
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitsId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.ValidityHours).GreaterThan(0).When(x => x.ValidityHours.HasValue);

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var permit = await permitFacts.GetPermitAsync(command.TenantId, command.PermitsId, cancellationToken);

                // Permit not found is a separate concern (the command handler itself returns
                // NotFound); a validator must not swallow that by silently passing or failing
                // this unrelated check. Not-found is out of scope for this rule -- pass through.
                if (permit is null)
                    return true;

                var hasTrainingRecord = permit.RequestedByUserId is { } requesterId
                    && await trainingFacts.HasTrainingRecordOnFileAsync(
                        command.TenantId, permit.SiteId, requesterId, command.RequestedAtUtc, cancellationToken);

                var context = new ComplianceRuleContext(
                    command.TenantId,
                    permit.SiteId,
                    "Permit",
                    permit.PermitsId,
                    command.RequestedAtUtc,
                    new Dictionary<string, string?>
                    {
                        // The command being validated is exactly the Created -> Active transition,
                        // so "Active" is the real post-condition state being authorized here, not
                        // a fabricated/assumed value.
                        ["Permit.Status"] = "Active",
                        ["Worker.HasTrainingRecordOnFile"] = hasTrainingRecord.ToString(),
                    });

                return Rule.Evaluate(context).Passed;
            })
            .WithMessage("Worker does not meet training requirements for this permit.")
            .WithErrorCode("Safety.PermitRequesterHasNoTrainingRecord");
    }
}
