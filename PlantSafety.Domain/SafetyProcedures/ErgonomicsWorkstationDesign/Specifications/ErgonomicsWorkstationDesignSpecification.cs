// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for ErgonomicsWorkstationDesign aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Specifications;

/// <summary>Abstract base specification for <see cref="ErgonomicsWorkstationDesign"/> aggregate predicate expressions.</summary>
public abstract class ErgonomicsWorkstationDesignSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ErgonomicsWorkstationDesignSpecification And(ErgonomicsWorkstationDesignSpecification other) => new AndErgonomicsWorkstationDesignSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ErgonomicsWorkstationDesignSpecification Or(ErgonomicsWorkstationDesignSpecification other) => new OrErgonomicsWorkstationDesignSpecification(this, other);
}

internal sealed class AndErgonomicsWorkstationDesignSpecification(ErgonomicsWorkstationDesignSpecification left, ErgonomicsWorkstationDesignSpecification right) : ErgonomicsWorkstationDesignSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign, bool>>(body, param);
    }
}

internal sealed class OrErgonomicsWorkstationDesignSpecification(ErgonomicsWorkstationDesignSpecification left, ErgonomicsWorkstationDesignSpecification right) : ErgonomicsWorkstationDesignSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign, bool>>(body, param);
    }
}