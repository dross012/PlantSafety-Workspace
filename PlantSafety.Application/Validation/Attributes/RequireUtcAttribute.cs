// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Data annotation attribute to document UTC requirement on DateTimeOffset properties.
//   This attribute is for documentation/tooling only — FluentValidation is the enforcement mechanism.
// SchemaVersion: 1

namespace PlantSafety.Application.Validation.Attributes;

/// <summary>
/// Documents that a <see cref="DateTimeOffset"/> property must be UTC (offset = 0).
/// Enforcement is performed by <see cref="Extensions.ValidatorExtensions.MustBeUtc{T}"/>.
/// This attribute is informational and does not perform runtime validation itself.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class RequireUtcAttribute : Attribute
{
    /// <summary>Optional message to display in documentation/Swagger.</summary>
    public string Message { get; } = "This field must be in UTC (DateTimeKind.Utc or offset = 0).";
}
