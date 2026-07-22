// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishErgonomicsWorkstationDesignCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.PublishErgonomicsWorkstationDesignCommand"/>.</summary>
public sealed class PublishErgonomicsWorkstationDesignCommandValidator : AbstractValidator<Commands.PublishErgonomicsWorkstationDesignCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishErgonomicsWorkstationDesignCommand"/>.</summary>
    public PublishErgonomicsWorkstationDesignCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ErgonomicsWorkstationDesignId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
    }
}