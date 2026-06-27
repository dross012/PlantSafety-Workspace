/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               IRuleOfT.cs
    DESCRIPTION:        Canonical generic rule contract. Represents a rule that evaluates a value of type T.
                        MUST return a RuleResult and MUST NOT throw for expected failures.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.Rules;

/// <summary>
/// Canonical generic rule contract.
/// </summary>
public interface IRule<T>
{
    /// <summary>
    /// Evaluates the rule against the provided value.
    /// MUST NOT throw for expected rule failures.
    /// </summary>
    RuleResult Evaluate(T value);
}
