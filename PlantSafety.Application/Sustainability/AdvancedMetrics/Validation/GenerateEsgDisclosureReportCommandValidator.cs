// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GenerateEsgDisclosureReportCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for GenerateEsgDisclosureReportCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Validation;

/// <summary>Validates <see cref="Commands.GenerateEsgDisclosureReportCommand"/>.</summary>
public sealed class GenerateEsgDisclosureReportCommandValidator : AbstractValidator<Commands.GenerateEsgDisclosureReportCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.GenerateEsgDisclosureReportCommand"/>.</summary>
    public GenerateEsgDisclosureReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.SourceLedgerId).NotEmpty();
        RuleFor(x => x.Frameworks).NotEmpty();
        RuleFor(x => x.MethodologyVersion).NotEmpty().MaximumLength(100);
    }
}
