// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Extensions
//  Layer:          SharedKernel
//  File:           EnumerableExtensions.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Pure, deterministic IEnumerable extension methods.
//                  No side effects, no I/O. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Extensions;

/// <summary>Pure, deterministic IEnumerable extension methods.</summary>
public static class EnumerableExtensions
{
    /// <summary>Returns true if the sequence is null or contains no elements.</summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        => source is null || !source.Any();

    /// <summary>Returns an empty enumerable if the source is null.</summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
        => source ?? Enumerable.Empty<T>();

    /// <summary>
    /// Splits a sequence into batches of at most <paramref name="size"/> items.
    /// </summary>
    public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        var batch = new List<T>(size);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch.AsReadOnly();
                batch = new List<T>(size);
            }
        }
        if (batch.Count > 0) yield return batch.AsReadOnly();
    }

    /// <summary>
    /// Returns distinct elements by a key selector, preserving first occurrence order.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in source)
            if (seen.Add(keySelector(item)))
                yield return item;
    }

    /// <summary>Converts a sequence to a read-only list, avoiding double enumeration.</summary>
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
        => source is IReadOnlyList<T> existing ? existing : source.ToList().AsReadOnly();
}
