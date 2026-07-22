// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PermitExpiration aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Specifications;

/// <summary>Abstract base specification for <see cref="PermitExpiration"/> aggregate predicate expressions.</summary>
public abstract class PermitExpirationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PermitExpirationSpecification And(PermitExpirationSpecification other) => new AndPermitExpirationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PermitExpirationSpecification Or(PermitExpirationSpecification other) => new OrPermitExpirationSpecification(this, other);
}

internal sealed class AndPermitExpirationSpecification(PermitExpirationSpecification left, PermitExpirationSpecification right) : PermitExpirationSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration, bool>>(body, param);
    }
}

internal sealed class OrPermitExpirationSpecification(PermitExpirationSpecification left, PermitExpirationSpecification right) : PermitExpirationSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration, bool>>(body, param);
    }
}