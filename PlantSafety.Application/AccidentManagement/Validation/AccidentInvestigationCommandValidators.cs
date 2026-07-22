// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validators for the real Accident investigation workflow
//   commands. Enforce invariants and normalization boundaries only; never call repositories or
//   external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.AccidentManagement.Enums;

namespace PlantSafety.Application.AccidentManagement.Validation;

/// <summary>Validates <see cref="Commands.AssignAccidentInvestigatorCommand"/>.</summary>
public sealed class AssignAccidentInvestigatorCommandValidator : AbstractValidator<Commands.AssignAccidentInvestigatorCommand>
{
    /// <summary>Defines validation rules.</summary>
    public AssignAccidentInvestigatorCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccidentId).NotEmpty();
        RuleFor(x => x.InvestigatorId).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.RecordAccidentRootCauseCommand"/>.</summary>
public sealed class RecordAccidentRootCauseCommandValidator : AbstractValidator<Commands.RecordAccidentRootCauseCommand>
{
    /// <summary>Defines validation rules.</summary>
    public RecordAccidentRootCauseCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccidentId).NotEmpty();
        RuleFor(x => x.RecordedBy).NotEmpty();
        RuleFor(x => x.RootCauseCategory)
            .NotEmpty()
            .Must(RootCauseCategory.IsValid)
            .WithMessage("RootCauseCategory must be a recognized RootCauseCategory code (Appendix F -- Hazard Categories).");
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(4096);
    }
}

/// <summary>Validates <see cref="Commands.CreateAccidentCorrectiveActionCommand"/>.</summary>
public sealed class CreateAccidentCorrectiveActionCommandValidator : AbstractValidator<Commands.CreateAccidentCorrectiveActionCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CreateAccidentCorrectiveActionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccidentId).NotEmpty();
        RuleFor(x => x.ResponsiblePersonId).NotEmpty();
        RuleFor(x => x.CreatedBy).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.DueAtUtc).GreaterThan(DateTime.UtcNow.AddMinutes(-5));
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(CorrectiveActionCategory.IsValid)
            .WithMessage("Category must be a recognized CorrectiveActionCategory code (Appendix G).");
    }
}

/// <summary>Validates <see cref="Commands.CompleteAccidentCorrectiveActionCommand"/>.</summary>
public sealed class CompleteAccidentCorrectiveActionCommandValidator : AbstractValidator<Commands.CompleteAccidentCorrectiveActionCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CompleteAccidentCorrectiveActionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccidentId).NotEmpty();
        RuleFor(x => x.CorrectiveActionId).NotEmpty();
        RuleFor(x => x.CompletedBy).NotEmpty();
        RuleFor(x => x.CompletionEvidence).NotEmpty().MaximumLength(1024);
    }
}

/// <summary>Validates <see cref="Commands.FileAccidentRegulatoryReportCommand"/>.</summary>
public sealed class FileAccidentRegulatoryReportCommandValidator : AbstractValidator<Commands.FileAccidentRegulatoryReportCommand>
{
    /// <summary>Defines validation rules.</summary>
    public FileAccidentRegulatoryReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccidentId).NotEmpty();
        RuleFor(x => x.FiledBy).NotEmpty();
        RuleFor(x => x.RegulatoryAgency).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ReferenceNumber).NotEmpty().MaximumLength(128);
    }
}

/// <summary>Validates <see cref="Commands.CloseAccidentInvestigationCommand"/>.</summary>
public sealed class CloseAccidentInvestigationCommandValidator : AbstractValidator<Commands.CloseAccidentInvestigationCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CloseAccidentInvestigationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccidentId).NotEmpty();
        RuleFor(x => x.ClosedBy).NotEmpty();
    }
}
