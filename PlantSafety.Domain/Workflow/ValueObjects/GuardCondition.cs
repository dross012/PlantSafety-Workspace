// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           GuardCondition.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Deterministic guard-condition value object evaluated against a workflow
//                  instance's context data before a transition is allowed to fire (Phase 5.2
//                  transition rules: "guard conditions before a transition is allowed"). Pure,
//                  side-effect-free string comparison -- no external calls, no wall-clock reads.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Workflow.ValueObjects;

/// <summary>Comparison operator supported by a <see cref="GuardCondition"/>.</summary>
public enum GuardOperator
{
    /// <summary>Context value equals the expected value (ordinal, case-insensitive).</summary>
    Equals = 0,

    /// <summary>Context value does not equal the expected value.</summary>
    NotEquals = 1,

    /// <summary>Context value, parsed as a decimal, is greater than the expected value.</summary>
    GreaterThan = 2,

    /// <summary>Context value, parsed as a decimal, is less than the expected value.</summary>
    LessThan = 3,

    /// <summary>Context value contains the expected value as a substring.</summary>
    Contains = 4,
}

/// <summary>
/// A single deterministic guard condition, evaluated against a workflow instance's context data
/// dictionary. Composed of a field name to look up, a comparison operator, and an expected value.
/// </summary>
public readonly record struct GuardCondition
{
    /// <summary>Context data key to evaluate. Empty for the always-true "no condition" instance.</summary>
    public string FieldName { get; }

    /// <summary>Comparison operator to apply.</summary>
    public GuardOperator Operator { get; }

    /// <summary>Expected value to compare the context field against.</summary>
    public string ExpectedValue { get; }

    private GuardCondition(string fieldName, GuardOperator op, string expectedValue)
    {
        FieldName = fieldName;
        Operator = op;
        ExpectedValue = expectedValue;
    }

    /// <summary>Creates a guard condition comparing the named context field against an expected value.</summary>
    public static GuardCondition Create(string fieldName, GuardOperator op, string expectedValue)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Field name must not be empty.", nameof(fieldName));
        return new GuardCondition(fieldName.Trim(), op, expectedValue ?? string.Empty);
    }

    /// <summary>The always-satisfied "no condition" guard, used for unconditional/default transitions.</summary>
    public static GuardCondition None => new(string.Empty, GuardOperator.Equals, string.Empty);

    /// <summary>Whether this instance represents the always-satisfied "no condition" guard.</summary>
    public bool IsUnconditional => FieldName.Length == 0;

    /// <summary>
    /// Deterministically evaluates this guard against the supplied context data. Missing context
    /// keys evaluate to an empty string. Always returns <see langword="true"/> for
    /// <see cref="None"/>.
    /// </summary>
    public bool Evaluate(IReadOnlyDictionary<string, string> context)
    {
        if (IsUnconditional)
            return true;

        var actual = context.TryGetValue(FieldName, out var v) ? v : string.Empty;

        return Operator switch
        {
            GuardOperator.Equals => string.Equals(actual, ExpectedValue, StringComparison.OrdinalIgnoreCase),
            GuardOperator.NotEquals => !string.Equals(actual, ExpectedValue, StringComparison.OrdinalIgnoreCase),
            GuardOperator.Contains => actual.Contains(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            GuardOperator.GreaterThan => TryCompareNumeric(actual, ExpectedValue, out var cmp) && cmp > 0,
            GuardOperator.LessThan => TryCompareNumeric(actual, ExpectedValue, out var cmp) && cmp < 0,
            _ => false,
        };
    }

    private static bool TryCompareNumeric(string left, string right, out int comparison)
    {
        comparison = 0;
        if (!decimal.TryParse(left, out var l) || !decimal.TryParse(right, out var r))
            return false;
        comparison = l.CompareTo(r);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
        => IsUnconditional ? "(always)" : $"{FieldName} {Operator} {ExpectedValue}";
}
