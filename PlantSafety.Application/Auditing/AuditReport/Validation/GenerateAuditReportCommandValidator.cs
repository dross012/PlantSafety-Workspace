// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: FluentValidation validator for GenerateAuditReportCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.Auditing.AuditReport.Enums;

namespace PlantSafety.Application.Auditing.AuditReport.Validation;

/// <summary>Validates <see cref="Commands.GenerateAuditReportCommand"/>.</summary>
public sealed class GenerateAuditReportCommandValidator : AbstractValidator<Commands.GenerateAuditReportCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.GenerateAuditReportCommand"/>.</summary>
    public GenerateAuditReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedBy).NotEmpty();
        RuleFor(x => x.TargetEntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetEntityId).NotEmpty();
        RuleFor(x => x.Format).NotEmpty().Must(AuditReportFormat.IsValid)
            .WithMessage("Format must be one of: Pdf, Csv, Json.");
        RuleFor(x => x.EntityIdentityJson).NotEmpty();
        RuleFor(x => x.ApprovalsJson).NotEmpty();
        RuleFor(x => x.ExceptionsJson).NotEmpty();
        RuleFor(x => x.SignaturesJson).NotEmpty();
    }
}
