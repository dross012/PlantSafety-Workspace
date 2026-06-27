// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Validation
//  Layer:          SharedKernel
//  File:           ValidationError.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Immutable validation error for a single property or field.
//                  Aggregated by validators and returned as a validation envelope.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Validation;

/// <summary>
/// Immutable validation error for a single property or field.
/// Multiple <see cref="ValidationError"/> instances are collected and returned
/// as a unified validation failure envelope.
/// </summary>
public sealed record ValidationError
{
    /// <summary>Property or field name (camelCase).</summary>
    public string PropertyName { get; }

    /// <summary>Human-readable, SIEM-safe error message.</summary>
    public string ErrorMessage { get; }

    /// <summary>Machine-readable error code for consumer discrimination.</summary>
    public string ErrorCode { get; }

    public ValidationError(string propertyName, string errorMessage, string errorCode = "VALIDATION_FAILURE")
    {
        PropertyName = propertyName?.Trim() ?? string.Empty;
        ErrorMessage = errorMessage?.Trim() ?? string.Empty;
        ErrorCode    = errorCode?.Trim()    ?? "VALIDATION_FAILURE";
    }

    public static ValidationError For(string property, string message)
        => new(property, message);

    public static ValidationError Required(string property)
        => new(property, $"{property} is required.", "REQUIRED");

    public static ValidationError MaxLength(string property, int max)
        => new(property, $"{property} must not exceed {max} characters.", "MAX_LENGTH");

    public override string ToString() => $"[{ErrorCode}] {PropertyName}: {ErrorMessage}";
}
