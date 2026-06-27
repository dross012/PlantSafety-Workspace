// =====================================================================================================
//  FILE: PropertyValidator.cs
//  AUTHOR: David Ross & Copilot
//  CREATED: 2026-05-27
//
//  PURPOSE:
//      Provides the abstract base class for all property-level validators in the Application layer.
//      This class implements the common scaffolding required by the 2026 Enterprise Validation
//      Pipeline, including deterministic execution, structural-only validation, and consistent
//      error creation.
//
//  RATIONALE & ONBOARDING NOTES:
//      - All validators inherit from this base class to ensure consistent behavior.
//      - Validators MUST be pure, deterministic, and side-effect free.
//      - Validators MUST NOT contain business rules; only structural validation is allowed.
//      - PropertyName is required and must match the request property being validated.
//      - Validate(object request) is sealed to enforce the template method pattern.
//      - Derived classes implement ValidateCore to perform actual validation logic.
//      - ValidationError objects are immutable and grouped by property name.
//
//  ENTERPRISE HANDBOOK COMPLIANCE:
//      ✔ Structural-only validation (shape, safety, metadata)
//      ✔ Deterministic execution (no randomness, no time-based logic)
//      ✔ Concurrency-safe (no shared mutable state)
//      ✔ Immutable error results
//      ✔ Template method pattern for consistency
//
//  DOMAIN NOTES:
//      - Used across all modules: Alerts, Routing, Scheduling, Auditing, Maintenance, etc.
//      - ValidationBehavior executes all validators concurrently and aggregates errors.
//      - Validators must be safe for replay and audit.
//
// =====================================================================================================
namespace PlantSafety.SharedKernel.Validation;

/// <summary>
/// Base class for all property-level validators. Implements the template method pattern
/// and enforces structural-only validation rules.
/// </summary>
public abstract class PropertyValidator : IPropertyValidator
{
    /// <summary>
    /// The name of the property this validator is responsible for.
    /// Must match the request property name exactly.
    /// </summary>
    public string PropertyName { get; }

    protected PropertyValidator(string propertyName)
    {
        PropertyName = propertyName
            ?? throw new ArgumentNullException(nameof(propertyName));
    }

    /// <summary>
    /// Executes validation in a deterministic, side-effect-free manner.
    /// This method is sealed to enforce the template method pattern.
    /// </summary>
    public IEnumerable<ValidationError> Validate(object request)
    {
        if (request is null)
            yield break;

        foreach (var error in ValidateCore(request))
            yield return error;
    }

    /// <summary>
    /// Derived classes implement this to perform actual validation logic.
    /// Must return zero or more ValidationError instances.
    /// </summary>
    protected abstract IEnumerable<ValidationError> ValidateCore(object request);

    /// <summary>
    /// Helper for creating a ValidationError bound to this validator's property.
    /// </summary>
    protected ValidationError Error(string code, string message)
        => new()
        { PropertyName = PropertyName, ErrorMessage = message };
}
