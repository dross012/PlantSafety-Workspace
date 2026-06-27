// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Extension methods that add reusable, domain-aware FluentValidation rules.
//   Each extension encapsulates a single cross-cutting validation concern.
//   Validators call these methods rather than duplicating logic.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Validation.Extensions;

/// <summary>
/// Reusable FluentValidation rule extensions for PlantSafety domain validations.
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Validates that a <see cref="Guid"/> property is not the default (empty) GUID.
    /// Applies the standard error message format used across all modules.
    /// </summary>
    public static IRuleBuilderOptions<T, Guid> IsValidId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .NotEqual(Guid.Empty)
            .WithMessage($"{fieldName} must be a valid non-empty identifier.");
    }

    /// <summary>
    /// Validates that a <see cref="DateTimeOffset"/> property is in UTC (offset = 0).
    /// </summary>
    public static IRuleBuilderOptions<T, DateTimeOffset> MustBeUtc<T>(
        this IRuleBuilder<T, DateTimeOffset> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .Must(dt => dt.Offset == TimeSpan.Zero)
            .WithMessage($"{fieldName} must be in UTC (offset must be zero).");
    }

    /// <summary>
    /// Validates that a nullable <see cref="DateTimeOffset"/> property is in UTC when it has a value.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTimeOffset?> MustBeUtcWhenProvided<T>(
        this IRuleBuilder<T, DateTimeOffset?> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .Must(dt => dt is null || dt.Value.Offset == TimeSpan.Zero)
            .WithMessage($"{fieldName} must be in UTC when provided.");
    }

    /// <summary>
    /// Validates that a <see cref="DateTimeOffset"/> is not in the future (with 30-second tolerance).
    /// </summary>
    public static IRuleBuilderOptions<T, DateTimeOffset> MustNotBeInFuture<T>(
        this IRuleBuilder<T, DateTimeOffset> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow.AddSeconds(30))
            .WithMessage($"{fieldName} cannot be in the future.");
    }

    /// <summary>
    /// Validates that a string is a valid ISO 4217 currency code (3 uppercase letters).
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidCurrencyCode<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency code must be a valid ISO 4217 code (3 uppercase letters, e.g., USD).");
    }

    /// <summary>
    /// Validates that a decimal value is positive (greater than zero).
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> MustBePositiveAmount<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .GreaterThan(0m)
            .WithMessage($"{fieldName} must be greater than zero.");
    }
}
