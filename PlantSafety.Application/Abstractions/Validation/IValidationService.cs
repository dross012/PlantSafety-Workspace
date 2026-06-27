// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Application-layer validation service abstraction.
//   Wraps FluentValidation registration and invocation for use in pipeline behaviors.
//   Validators MUST only throw ValidationException; they must never log, mutate, or call repositories.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

using FluentValidation.Results;

namespace PlantSafety.Application.Abstractions.Validation;

/// <summary>
/// Provides programmatic validation of request objects.
/// Used by <c>ValidationBehavior</c> to validate all commands and queries before handler execution.
/// All validators enforce invariants, cross-field rules, date range correctness, and strong-typed value non-emptiness.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates the given instance using all registered FluentValidation validators.
    /// Returns an empty result if no validators are registered for the type.
    /// Never throws; returns failures in the result.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    /// <param name="instance">The object to validate.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>A <see cref="ValidationResult"/> containing all failures, or an empty result on success.</returns>
    Task<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the given instance and throws <see cref="FluentValidation.ValidationException"/>
    /// if any validation failures are found.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    /// <param name="instance">The object to validate.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task ValidateAndThrowAsync<T>(T instance, CancellationToken cancellationToken = default);
}
