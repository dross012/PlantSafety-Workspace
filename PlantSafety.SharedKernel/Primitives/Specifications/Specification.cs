/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               Specification.cs
    DESCRIPTION:        Canonical base class for specifications. Provides default implementations for expression handling,
                        in-memory evaluation, and combinator creation (And, Or, Not).

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Linq.Expressions;

namespace PlantSafety.SharedKernel.Primitives.Specifications;

/// <summary>
/// Canonical base class for specifications.
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T value)
        => ToExpression().Compile().Invoke(value);

    public Specification<T> And(ISpecification<T> other)
        => new AndSpecification<T>(this, other);

    public Specification<T> Or(ISpecification<T> other)
        => new OrSpecification<T>(this, other);

    public Specification<T> Not()
        => new NotSpecification<T>(this);
}
