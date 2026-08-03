// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           ProductionTrackingCommandValidators.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validators for every Operational.ProductionTracking command.
//                  Consolidated into one file per the sibling WorkOrderCommandValidators.cs
//                  precedent of grouping closely-related small types. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Additive-only evolution.
// ============================================================================
using FluentValidation;
using PlantSafety.Application.Operational.ProductionTracking.Commands;

namespace PlantSafety.Application.Operational.ProductionTracking.Validation;

/// <summary>Validates <see cref="CreateProductionLineCommand"/>.</summary>
public sealed class CreateProductionLineCommandValidator : AbstractValidator<CreateProductionLineCommand>
{
    /// <summary>Defines validation rules for <see cref="CreateProductionLineCommand"/>.</summary>
    public CreateProductionLineCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.LineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LineName).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Validates <see cref="StartProductionRunCommand"/>.</summary>
public sealed class StartProductionRunCommandValidator : AbstractValidator<StartProductionRunCommand>
{
    /// <summary>Defines validation rules for <see cref="StartProductionRunCommand"/>.</summary>
    public StartProductionRunCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.RunCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.LineName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MachineId).NotEmpty();
        RuleFor(x => x.OperatorName).MaximumLength(200);
        RuleFor(x => x.IdealCycleTimeSeconds).GreaterThan(0);
    }
}

/// <summary>Validates <see cref="RecordProductionRunEventCommand"/>.</summary>
public sealed class RecordProductionRunEventCommandValidator : AbstractValidator<RecordProductionRunEventCommand>
{
    /// <summary>Defines validation rules for <see cref="RecordProductionRunEventCommand"/>.</summary>
    public RecordProductionRunEventCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RunId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.RunEventType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DowntimeReasonCode).MaximumLength(50);
        RuleFor(x => x.DowntimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GoodUnitsDelta).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ScrapUnitsDelta).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DowntimeReasonCode).NotEmpty().When(x => x.IsDowntime)
            .WithMessage("A downtime reason code is required when IsDowntime is true.");
    }
}

/// <summary>Validates <see cref="AddProductionBatchCommand"/>.</summary>
public sealed class AddProductionBatchCommandValidator : AbstractValidator<AddProductionBatchCommand>
{
    /// <summary>Defines validation rules for <see cref="AddProductionBatchCommand"/>.</summary>
    public AddProductionBatchCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RunId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.BatchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => Enum.TryParse<Domain.Operational.ProductionTracking.Enums.BatchStatus>(s, true, out _))
            .WithMessage("Status must be one of Pending, Active, Completed, Failed.");
    }
}

/// <summary>Validates <see cref="CompleteProductionRunCommand"/>.</summary>
public sealed class CompleteProductionRunCommandValidator : AbstractValidator<CompleteProductionRunCommand>
{
    /// <summary>Defines validation rules for <see cref="CompleteProductionRunCommand"/>.</summary>
    public CompleteProductionRunCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RunId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
    }
}
