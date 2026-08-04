// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           DefineTaskComplexityProfileCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for DefineTaskComplexityProfileCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

/// <summary>Validates <see cref="Commands.DefineTaskComplexityProfileCommand"/>.</summary>
public sealed class DefineTaskComplexityProfileCommandValidator : AbstractValidator<Commands.DefineTaskComplexityProfileCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DefineTaskComplexityProfileCommand"/>.</summary>
    public DefineTaskComplexityProfileCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ShiftType).NotEmpty().Must(s => Enum.TryParse<Domain.HumanFactors.CognitiveLoadAnalytics.Enums.ShiftType>(s, true, out _))
            .WithMessage("ShiftType must be one of: Day, Evening, Night, Rotating.");
        RuleFor(x => x.TaskCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TaskName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ComplexityScore).InclusiveBetween(1, 10);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
