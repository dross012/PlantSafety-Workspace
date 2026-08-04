// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for CognitiveLoadLedger aggregate predicate
//                  expressions. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.CognitiveLoadLedger"/> aggregate predicate expressions.</summary>
public abstract class CognitiveLoadLedgerSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.CognitiveLoadLedger, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.CognitiveLoadLedger, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CognitiveLoadLedgerSpecification And(CognitiveLoadLedgerSpecification other) => new AndCognitiveLoadLedgerSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CognitiveLoadLedgerSpecification Or(CognitiveLoadLedgerSpecification other) => new OrCognitiveLoadLedgerSpecification(this, other);
}

internal sealed class AndCognitiveLoadLedgerSpecification(CognitiveLoadLedgerSpecification left, CognitiveLoadLedgerSpecification right) : CognitiveLoadLedgerSpecification
{
    public override Expression<Func<Aggregates.CognitiveLoadLedger, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.CognitiveLoadLedger, bool>>(body, param);
    }
}

internal sealed class OrCognitiveLoadLedgerSpecification(CognitiveLoadLedgerSpecification left, CognitiveLoadLedgerSpecification right) : CognitiveLoadLedgerSpecification
{
    public override Expression<Func<Aggregates.CognitiveLoadLedger, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.CognitiveLoadLedger, bool>>(body, param);
    }
}
