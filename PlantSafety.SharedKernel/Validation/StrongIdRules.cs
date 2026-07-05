// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Shared Kernel
//  Module:         Validation
//  Layer:          SharedKernel (Rules)
//  File:           StrongIdRules.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      FluentValidation adapters for StrongId types. These rules ensure that
//      all StrongId-based identifiers are non-empty, well-formed, and safe.
//
//  Architectural Notes:
//      • StrongId MUST NOT allow Guid.Empty (Rule 3.2).
//      • StrongId MUST be validated at the Application boundary (Rule 2.5).
//
//  Onboarding Notes:
//      • These rules are used by all command/query validators.
// ============================================================================

using FluentValidation;
using PlantSafety.SharedKernel.Primitives.StrongIds;

namespace PlantSafety.SharedKernel.Validation;

public static class StrongIdRules
{
    public static IPropertyValidator CorrelationId() =>
        new StrongIdValidator("CorrelationId");

    public static IPropertyValidator NullableCausationId() =>
        new NullableStrongIdValidator("CausationId");

    public static IPropertyValidator AlertId() =>
        new StrongIdValidator("AlertId");

    public static IPropertyValidator UserId() =>
        new StrongIdValidator("UserId");

    private sealed class StrongIdValidator : FluentValidation.Validators.PropertyValidator<IStrongId, IStrongId>, IPropertyValidator
    {
        private readonly string _name;

        public StrongIdValidator(string name) => _name = name;

        public override string Name => "StrongIdValidator";

        public override bool IsValid(ValidationContext<IStrongId> context, IStrongId value)
        {
            if (value.Value == Guid.Empty)
            {
                context.AddFailure($"{_name} must not be empty.");
                return false;
            }

            return true;
        }

        // Implement IPropertyValidator members
        public string PropertyName => _name;

        public IEnumerable<ValidationError> Validate(object request)
        {
            if (request is not IStrongId value)
                yield break;

            if (value.Value == Guid.Empty)
                yield return new ValidationError(_name, $"{_name} must not be empty.");
        }
    }

    private sealed class NullableStrongIdValidator : FluentValidation.Validators.PropertyValidator<IStrongId?, IStrongId?>, IPropertyValidator
    {
        private readonly string _name;

        public NullableStrongIdValidator(string name) => _name = name;

        public override string Name => "NullableStrongIdValidator";

        public override bool IsValid(ValidationContext<IStrongId?> context, IStrongId? value)
        {
            if (value is null)
                return true;

            if (value.Value == Guid.Empty)
            {
                context.AddFailure($"{_name} must not be empty when provided.");
                return false;
            }

            return true;
        }

        // Implement IPropertyValidator members
        public string PropertyName => _name;

        public IEnumerable<ValidationError> Validate(object request)
        {
            if (request is not IStrongId value)
                yield break;

            if (value.Value == Guid.Empty)
                yield return new ValidationError(_name, $"{_name} must not be empty when provided.");
        }
    }
}

