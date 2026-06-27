/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               NotSpecification.cs
    DESCRIPTION:        Logical NOT combinator for specifications. MUST preserve EF-safe expression tree semantics and
                        deterministic behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Linq.Expressions;

namespace PlantSafety.SharedKernel.Primitives.Specifications;

/// <summary>
/// Logical NOT combinator for specifications.
/// </summary>
public sealed class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _inner;

    public NotSpecification(ISpecification<T> inner)
    {
        _inner = inner;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = _inner.ToExpression();
        var param = Expression.Parameter(typeof(T));

        var body = Expression.Not(Expression.Invoke(expr, param));

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
