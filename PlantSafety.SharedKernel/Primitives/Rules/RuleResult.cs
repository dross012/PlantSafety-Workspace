/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               RuleResult.cs
    DESCRIPTION:        Canonical rule result primitive. Represents the outcome of a rule evaluation, including type,
                        message, and optional metadata. MUST remain immutable and SIEM-safe.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Rules;

/// <summary>
/// Canonical rule result.
/// </summary>
public sealed record RuleResult
{
    /// <summary>
    /// Classification of the rule result.
    /// </summary>
    public RuleResultType Type { get; }

    /// <summary>
    /// Optional SIEM-safe message describing the rule outcome.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Optional error associated with the rule failure.
    /// </summary>
    public Error? Error { get; }

    private RuleResult(RuleResultType type, string message, Error? error)
    {
        Type = type;
        Message = message;
        Error = error;
    }

    public static RuleResult Success(string message = "Rule passed.")
        => new(RuleResultType.Success, message, null);

    public static RuleResult Failure(Error error, string? message = null)
        => new(RuleResultType.Failure, message ?? error.Message, error);

    public static RuleResult InvalidContext(string message)
        => new(RuleResultType.InvalidContext, message, null);

    public override string ToString()
        => $"{Type}: {Message}";
}
