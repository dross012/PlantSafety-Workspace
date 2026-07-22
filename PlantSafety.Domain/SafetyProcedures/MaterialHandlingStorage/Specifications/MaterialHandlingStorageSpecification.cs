// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for MaterialHandlingStorage aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Specifications;

/// <summary>Abstract base specification for <see cref="MaterialHandlingStorage"/> aggregate predicate expressions.</summary>
public abstract class MaterialHandlingStorageSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public MaterialHandlingStorageSpecification And(MaterialHandlingStorageSpecification other) => new AndMaterialHandlingStorageSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public MaterialHandlingStorageSpecification Or(MaterialHandlingStorageSpecification other) => new OrMaterialHandlingStorageSpecification(this, other);
}

internal sealed class AndMaterialHandlingStorageSpecification(MaterialHandlingStorageSpecification left, MaterialHandlingStorageSpecification right) : MaterialHandlingStorageSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage, bool>>(body, param);
    }
}

internal sealed class OrMaterialHandlingStorageSpecification(MaterialHandlingStorageSpecification left, MaterialHandlingStorageSpecification right) : MaterialHandlingStorageSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage, bool>>(body, param);
    }
}