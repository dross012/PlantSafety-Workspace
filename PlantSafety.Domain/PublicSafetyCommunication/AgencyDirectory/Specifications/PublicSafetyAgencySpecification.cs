// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencySpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PublicSafetyAgency aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Specifications;

/// <summary>Abstract base specification for <see cref="PublicSafetyAgency"/> aggregate predicate expressions.</summary>
public abstract class PublicSafetyAgencySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PublicSafetyAgencySpecification And(PublicSafetyAgencySpecification other) => new AndPublicSafetyAgencySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PublicSafetyAgencySpecification Or(PublicSafetyAgencySpecification other) => new OrPublicSafetyAgencySpecification(this, other);
}

internal sealed class AndPublicSafetyAgencySpecification(PublicSafetyAgencySpecification left, PublicSafetyAgencySpecification right) : PublicSafetyAgencySpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency, bool>>(body, param);
    }
}

internal sealed class OrPublicSafetyAgencySpecification(PublicSafetyAgencySpecification left, PublicSafetyAgencySpecification right) : PublicSafetyAgencySpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency, bool>>(body, param);
    }
}
