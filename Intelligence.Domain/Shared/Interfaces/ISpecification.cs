/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ISpecification.cs
    DESCRIPTION:        Generic specification interface for in-memory domain predicate evaluation.
                        Specifications express domain rules as named, reusable predicates. They are NOT
                        used to construct database queries — that responsibility belongs to query objects
                        in the Application layer. (Rule 2.9)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.Shared.Interfaces;

/// <summary>
/// Expresses a named, reusable predicate over a domain entity or aggregate.
/// Used exclusively for in-memory evaluation within the domain layer.
/// </summary>
/// <typeparam name="T">The domain type this specification evaluates.</typeparam>
public interface ISpecification<T>
{
    /// <summary>Evaluates whether the given entity satisfies this specification.</summary>
    bool IsSatisfiedBy(T entity);

    /// <summary>Human-readable description of the rule this specification encodes.</summary>
    string Description { get; }
}
