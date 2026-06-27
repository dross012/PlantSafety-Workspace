/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               RuleResultType.cs
    DESCRIPTION:        Canonical rule result classification. MUST remain additive-only and deterministic.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.Rules;

/// <summary>
/// Canonical rule result classification.
/// </summary>
public enum RuleResultType
{
    /// <summary>
    /// Rule passed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Rule failed due to a business or domain condition.
    /// </summary>
    Failure = 1,

    /// <summary>
    /// Rule could not be evaluated due to missing or invalid context.
    /// </summary>
    InvalidContext = 2,

    /// <summary>
    ///  Skipped due to unmet preconditions, such as a prerequisite rule failure.
    /// </summary>
    Skipped = 3
}
