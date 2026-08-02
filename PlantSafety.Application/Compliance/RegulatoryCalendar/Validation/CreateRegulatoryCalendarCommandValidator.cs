// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           CreateRegulatoryCalendarCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateRegulatoryCalendarCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Validation;

/// <summary>Validates <see cref="Commands.CreateRegulatoryCalendarCommand"/>.</summary>
public sealed class CreateRegulatoryCalendarCommandValidator : AbstractValidator<Commands.CreateRegulatoryCalendarCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateRegulatoryCalendarCommand"/>.</summary>
    public CreateRegulatoryCalendarCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ResponsiblePartyId).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.EventTitle).NotEmpty().MaximumLength(400);
        RuleFor(x => x.RegulatoryBody).NotEmpty().MaximumLength(400);
        RuleFor(x => x.JurisdictionCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ComplianceCategory).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReminderLeadDays).GreaterThanOrEqualTo(0);
    }
}
