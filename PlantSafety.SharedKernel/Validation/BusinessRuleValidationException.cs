// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Validation
//  Layer:          SharedKernel
//  File:           BusinessRuleValidationException.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Exception raised when a domain business rule is violated.
//                  Thrown by BusinessRuleValidator.Check().
//                  Maps to HTTP 422 Unprocessable Content at the API boundary.
//                  SIEM-safe: no stack traces, no PII in message. Additive-only.
// ============================================================================
namespace PlantSafety.SharedKernel.Validation;

/// <summary>
/// Raised when an <see cref="IBusinessRule"/> is violated.
/// Maps to HTTP 422 Unprocessable Content at the API boundary.
/// </summary>
public sealed class BusinessRuleValidationException : Exception
{
    /// <summary>The business rule that was violated.</summary>
    public IBusinessRule BrokenRule { get; }

    /// <summary>Machine-readable rule identifier (class name of the broken rule).</summary>
    public string RuleId { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
        RuleId     = brokenRule.GetType().Name;
    }

    public override string ToString() => $"[{RuleId}] {Message}";
}
