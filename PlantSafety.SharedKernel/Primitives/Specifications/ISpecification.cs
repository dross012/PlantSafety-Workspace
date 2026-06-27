/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               ISpecificationOfT.cs
    DESCRIPTION:        Canonical generic specification contract. Represents a predicate over a value of type T. MUST be
                        EF-safe, Dapper-safe, and deterministic.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Linq.Expressions;

namespace PlantSafety.SharedKernel.Primitives.Specifications;

/// <summary>
/// Canonical generic specification.
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// Expression representing the specification predicate.
    /// MUST be pure, deterministic, and EF-safe.
    /// </summary>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Compiled predicate for in-memory evaluation.
    /// </summary>
    bool IsSatisfiedBy(T value);
}
