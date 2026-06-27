// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Validation
//  Layer:          SharedKernel
//  File:           IBusinessRule.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Contract for domain business rules checked by aggregate methods.
//                  Business rules express domain invariants; violation throws a
//                  domain exception — NOT a validation exception.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Validation;

/// <summary>
/// Contract for domain business rules.
/// Business rules encode domain invariants that MUST hold at all times.
/// Violation raises a domain exception, not a validation error.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Returns <c>true</c> when the rule is violated (invariant is broken).
    /// </summary>
    bool IsBroken();

    /// <summary>Human-readable, SIEM-safe description of the violated invariant.</summary>
    string Message { get; }
}
