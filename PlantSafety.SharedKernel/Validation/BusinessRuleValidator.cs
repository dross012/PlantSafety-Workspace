// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Validation
//  Layer:          SharedKernel
//  File:           BusinessRuleValidator.cs
//  Created:        2026-07-26
//  SchemaVersion:  2026.1
//  Description:    Static gatekeeper that checks an IBusinessRule and throws
//                  BusinessRuleValidationException when the rule is broken.
//                  correlationId/causationId are accepted for future SIEM-safe
//                  logging correlation but are not otherwise used yet.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.SharedKernel.Validation;

/// <summary>
/// Static gatekeeper for domain business rules.
/// Call <see cref="Check"/> from aggregate methods to enforce an <see cref="IBusinessRule"/>.
/// </summary>
public static class BusinessRuleValidator
{
    /// <summary>
    /// Evaluates <paramref name="rule"/> and throws <see cref="BusinessRuleValidationException"/>
    /// when it is broken. No-op when the rule passes.
    /// </summary>
    /// <param name="rule">The business rule to evaluate.</param>
    /// <param name="correlationId">Correlation identifier for the operation being validated.</param>
    /// <param name="causationId">Causation identifier for the operation being validated.</param>
    public static void Check(IBusinessRule rule, CorrelationId correlationId, CausationId causationId)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}
