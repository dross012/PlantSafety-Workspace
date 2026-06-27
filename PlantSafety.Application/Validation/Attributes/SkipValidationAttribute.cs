// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Marker attribute that instructs ConditionalValidationBehavior to bypass
//   FluentValidation for the decorated request type.  Use sparingly and only for internal
//   system-generated commands that cannot carry user-supplied data.
// SchemaVersion: 1

namespace PlantSafety.Application.Validation.Attributes;

/// <summary>
/// Marks a MediatR request type to skip FluentValidation in the pipeline.
/// Only apply to internally generated commands (e.g. background job commands, saga steps)
/// where all fields are guaranteed to be structurally valid.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class SkipValidationAttribute : Attribute
{
    /// <summary>Reason the validation skip is justified.  Required for auditability.</summary>
    public string Reason { get; }

    /// <summary>Initialises the attribute with the justification reason.</summary>
    public SkipValidationAttribute(string reason)
    {
        Reason = reason;
    }
}
