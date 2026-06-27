// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Marker interface for all explicit, one-directional mapping profiles.
//   §1.3 (Canonical Mapping Rule): all mapping must be explicit, deterministic, and one-way per layer boundary.
//   AutoMapper and reflection-based mapping are prohibited (§2.16 Prohibited Technology Rule).
//   Every Command → Domain → DTO path must have a corresponding mapping profile.
// SchemaVersion: 1
// Additive-only evolution: new methods may be added; no existing methods may be removed or renamed.

namespace PlantSafety.Application.Common.Mapping;

/// <summary>
/// Marker interface for explicit, one-directional Application layer mapping profiles.
/// Mapping is always hand-written and deterministic; AutoMapper is prohibited.
/// One mapping profile per layer boundary (e.g., DTO → Command, Domain → ResponseDto).
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public interface IMappingProfile<in TSource, out TDestination>
{
    /// <summary>
    /// Maps the given source instance to a new destination instance.
    /// All fields must be explicitly assigned; no silent drops, no inference.
    /// </summary>
    /// <param name="source">The source instance to map.</param>
    /// <returns>A new, fully-populated destination instance.</returns>
    TDestination Map(TSource source);
}

/// <summary>
/// Marker interface for collection mapping profiles.
/// </summary>
/// <typeparam name="TSource">The source item type.</typeparam>
/// <typeparam name="TDestination">The destination item type.</typeparam>
public interface ICollectionMappingProfile<in TSource, out TDestination>
{
    /// <summary>
    /// Maps a collection of source items to a read-only list of destination items.
    /// Never returns null; returns an empty list on null or empty input.
    /// </summary>
    /// <param name="sources">The source collection.</param>
    /// <returns>A non-null, read-only list of mapped destination items.</returns>
    IReadOnlyList<TDestination> MapCollection(IEnumerable<TSource>? sources);
}
