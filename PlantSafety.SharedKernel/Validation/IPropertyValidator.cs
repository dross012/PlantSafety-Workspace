// =====================================================================================================
//  FILE: IPropertyValidator.cs
//  AUTHOR: David Ross & Copilot
//  CREATED: 2026-05-27
//
//  PURPOSE:
//      Defines the contract for all property-level validators used by the Application layer.
//      Each validator enforces structural validation rules for a single property or a small
//      group of related properties. Validators implementing this interface are executed by the
//      ValidationBehavior in a concurrent, deterministic manner.
//
//  RATIONALE & ONBOARDING NOTES:
//      - This interface is part of the 2026 Enterprise Validation Pipeline.
//      - Validators MUST be pure, deterministic, and side-effect free.
//      - Validators MUST NOT contain business rules; only structural validation is allowed.
//      - Validators return ValidationError objects, which are grouped by property name.
//      - Validators run concurrently; therefore, they must not depend on shared mutable state.
//      - Validators must be safe for replay, audit, and deterministic execution.
//
//  ENTERPRISE HANDBOOK COMPLIANCE:
//      ✔ Structural-only validation (no business logic)
//      ✔ Deterministic, side-effect-free execution
//      ✔ Concurrency-safe (no shared state)
//      ✔ Immutable error results
//      ✔ Property-scoped validation responsibility
//
//  DOMAIN NOTES:
//      - Used across all modules: Alerts, Routing, Scheduling, Auditing, Maintenance, etc.
//      - ValidationBehavior aggregates all errors and returns a unified ValidationException.
//      - Validators are discoverable via DI and executed automatically for each request.
//
// =====================================================================================================

namespace PlantSafety.SharedKernel.Validation;


/// <summary>
/// Represents a validator responsible for validating a single property or a small group of
/// related properties. Validators implementing this interface must enforce structural validation
/// rules only (shape, safety, metadata), not business rules.
/// </summary>
public interface IPropertyValidator
{
    /// <summary>
    /// Gets the name of the property this validator is responsible for.
    /// This is used by the ValidationBehavior to group validation errors.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Validates the specified request instance and returns zero or more validation errors.
    /// Implementations must be pure, deterministic, and side-effect free.
    /// </summary>
    /// <param name="request">The request object being validated.</param>
    /// <returns>A collection of validation errors for the property.</returns>
    IEnumerable<ValidationError> Validate(object request);
}

public class ValidationError(string propertyName, string errorMessage)
{
    public string PropertyName { get; } = propertyName;
    public string ErrorMessage { get; } = errorMessage;
}
