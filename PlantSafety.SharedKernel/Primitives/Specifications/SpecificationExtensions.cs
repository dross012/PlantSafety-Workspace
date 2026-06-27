/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               SpecificationExtensions.cs
    DESCRIPTION:        Canonical extension helpers for specifications. Provides composition helpers and in-memory
                        evaluation shortcuts.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Collections.Generic;
using System.Linq;

namespace PlantSafety.SharedKernel.Primitives.Specifications;

/// <summary>
/// Extension helpers for specifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Filters a sequence using a specification.
    /// </summary>
    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, ISpecification<T> spec)
        => source.Where(x => spec.IsSatisfiedBy(x));

    /// <summary>
    /// Determines whether any element satisfies the specification.
    /// </summary>
    public static bool Any<T>(this IEnumerable<T> source, ISpecification<T> spec)
        => source.Any(x => spec.IsSatisfiedBy(x));

    /// <summary>
    /// Determines whether all elements satisfy the specification.
    /// </summary>
    public static bool All<T>(this IEnumerable<T> source, ISpecification<T> spec)
        => source.All(x => spec.IsSatisfiedBy(x));
}
