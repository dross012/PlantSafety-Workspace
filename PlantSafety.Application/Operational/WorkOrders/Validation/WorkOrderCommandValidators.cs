// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           WorkOrderCommandValidators.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validators for every Operational.WorkOrders command.
//                  Consolidated into one file per the sibling PreventiveMaintenanceTaskEvents.cs
//                  precedent of grouping closely-related small types. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Additive-only evolution.
// ============================================================================
using FluentValidation;
using PlantSafety.Application.Operational.WorkOrders.Commands;

namespace PlantSafety.Application.Operational.WorkOrders.Validation;

/// <summary>Validates <see cref="CreateWorkOrderCommand"/>.</summary>
public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    /// <summary>Defines validation rules for <see cref="CreateWorkOrderCommand"/>.</summary>
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.WorkOrderCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MachineId).NotEmpty();
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Priority).NotEmpty().Must(p => Enum.TryParse<Domain.Operational.WorkOrders.Enums.WorkOrderPriorityLevel>(p, true, out _))
            .WithMessage("Priority must be one of Low, Medium, High, Critical.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.DueAtUtc).NotEqual(default(DateTime));
    }
}

/// <summary>Validates <see cref="AssignWorkOrderCommand"/>.</summary>
public sealed class AssignWorkOrderCommandValidator : AbstractValidator<AssignWorkOrderCommand>
{
    /// <summary>Defines validation rules for <see cref="AssignWorkOrderCommand"/>.</summary>
    public AssignWorkOrderCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.TechnicianId).NotEmpty();
        RuleFor(x => x.TechnicianName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RequestedByUserId).NotEmpty();
    }
}

/// <summary>Validates <see cref="UpdateWorkOrderStatusCommand"/>.</summary>
public sealed class UpdateWorkOrderStatusCommandValidator : AbstractValidator<UpdateWorkOrderStatusCommand>
{
    private static readonly string[] AllowedTargets = ["start", "putonhold", "hold", "resume", "cancel"];

    /// <summary>Defines validation rules for <see cref="UpdateWorkOrderStatusCommand"/>.</summary>
    public UpdateWorkOrderStatusCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.TargetStatus).NotEmpty()
            .Must(t => AllowedTargets.Contains(t.Trim().ToLowerInvariant()))
            .WithMessage("TargetStatus must be one of Start, PutOnHold, Resume, Cancel.");
        RuleFor(x => x.Reason).MaximumLength(2000);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.TargetStatus.Trim().Equals("cancel", StringComparison.OrdinalIgnoreCase))
            .WithMessage("A reason is required when cancelling a work order.");
    }
}

/// <summary>Validates <see cref="CompleteWorkOrderCommand"/>.</summary>
public sealed class CompleteWorkOrderCommandValidator : AbstractValidator<CompleteWorkOrderCommand>
{
    /// <summary>Defines validation rules for <see cref="CompleteWorkOrderCommand"/>.</summary>
    public CompleteWorkOrderCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.CompletionNotes).MaximumLength(2000);
    }
}

/// <summary>Validates <see cref="AdjustPartStockCommand"/>.</summary>
public sealed class AdjustPartStockCommandValidator : AbstractValidator<AdjustPartStockCommand>
{
    /// <summary>Defines validation rules for <see cref="AdjustPartStockCommand"/>.</summary>
    public AdjustPartStockCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PartId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.DeltaQuantity).NotEqual(0).WithMessage("DeltaQuantity must be non-zero.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
