// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validators for the SafetyPolicy governance-lifecycle commands.
//   Enforce invariants and normalization boundaries only; never call repositories or external
//   services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.SafetyPolicies.Enums;

namespace PlantSafety.Application.SafetyPolicies.Validation;

/// <summary>Validates <see cref="Commands.SubmitSafetyPolicyForApprovalCommand"/>.</summary>
public sealed class SubmitSafetyPolicyForApprovalCommandValidator : AbstractValidator<Commands.SubmitSafetyPolicyForApprovalCommand>
{
    /// <summary>Defines validation rules.</summary>
    public SubmitSafetyPolicyForApprovalCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.SubmittedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.ApproveSafetyPolicyCommand"/>.</summary>
public sealed class ApproveSafetyPolicyCommandValidator : AbstractValidator<Commands.ApproveSafetyPolicyCommand>
{
    /// <summary>Defines validation rules.</summary>
    public ApproveSafetyPolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.ApprovedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.RejectSafetyPolicyCommand"/>.</summary>
public sealed class RejectSafetyPolicyCommandValidator : AbstractValidator<Commands.RejectSafetyPolicyCommand>
{
    /// <summary>Defines validation rules.</summary>
    public RejectSafetyPolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.RejectedBy).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
    }
}

/// <summary>Validates <see cref="Commands.PublishSafetyPolicyCommand"/>.</summary>
public sealed class PublishSafetyPolicyCommandValidator : AbstractValidator<Commands.PublishSafetyPolicyCommand>
{
    /// <summary>Defines validation rules.</summary>
    public PublishSafetyPolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.EffectiveAtUtc).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.AcknowledgeSafetyPolicyCommand"/>.</summary>
public sealed class AcknowledgeSafetyPolicyCommandValidator : AbstractValidator<Commands.AcknowledgeSafetyPolicyCommand>
{
    /// <summary>Defines validation rules.</summary>
    public AcknowledgeSafetyPolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.AcknowledgedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.StartSafetyPolicyReviewCommand"/>.</summary>
public sealed class StartSafetyPolicyReviewCommandValidator : AbstractValidator<Commands.StartSafetyPolicyReviewCommand>
{
    /// <summary>Defines validation rules.</summary>
    public StartSafetyPolicyReviewCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.StartedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.CompleteSafetyPolicyReviewCommand"/>.</summary>
public sealed class CompleteSafetyPolicyReviewCommandValidator : AbstractValidator<Commands.CompleteSafetyPolicyReviewCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CompleteSafetyPolicyReviewCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.ReviewedBy).NotEmpty();
        RuleFor(x => x.NewReviewCadence)
            .Must(code => string.IsNullOrWhiteSpace(code) || ReviewCadence.IsValid(code))
            .WithMessage("NewReviewCadence must be a recognized ReviewCadence code when supplied.");
    }
}

/// <summary>Validates <see cref="Commands.RetireSafetyPolicyCommand"/>.</summary>
public sealed class RetireSafetyPolicyCommandValidator : AbstractValidator<Commands.RetireSafetyPolicyCommand>
{
    /// <summary>Defines validation rules.</summary>
    public RetireSafetyPolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyPolicyId).NotEmpty();
        RuleFor(x => x.RetiredBy).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
    }
}
