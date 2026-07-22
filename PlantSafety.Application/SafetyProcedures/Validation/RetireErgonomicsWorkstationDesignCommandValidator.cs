// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireErgonomicsWorkstationDesignCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RetireErgonomicsWorkstationDesignCommand"/>.</summary>
public sealed class RetireErgonomicsWorkstationDesignCommandValidator : AbstractValidator<Commands.RetireErgonomicsWorkstationDesignCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireErgonomicsWorkstationDesignCommand"/>.</summary>
    public RetireErgonomicsWorkstationDesignCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ErgonomicsWorkstationDesignId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}