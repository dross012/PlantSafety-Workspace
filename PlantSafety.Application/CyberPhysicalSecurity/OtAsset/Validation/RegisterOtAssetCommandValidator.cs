// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterOtAssetCommand. Enforces invariants and
//   normalization boundaries only; never calls repositories or external services.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Validation;

/// <summary>Validates <see cref="Commands.RegisterOtAssetCommand"/>.</summary>
public sealed class RegisterOtAssetCommandValidator : AbstractValidator<Commands.RegisterOtAssetCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterOtAssetCommand"/>.</summary>
    public RegisterOtAssetCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.AssetTag).NotEmpty().MaximumLength(128);
        RuleFor(x => x.AssetName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.AssetType).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
