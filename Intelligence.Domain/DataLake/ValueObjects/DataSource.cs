/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               DataSource.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.DataLake.ValueObjects;

/// <summary>Identifies the origin of data in the Intelligence Data Lake. Never stores credentials.</summary>
public sealed class DataSource : ValueObject
{
    /// <summary>The originating system name (e.g., "PlantOperations").</summary>
    public string SystemName { get; }

    /// <summary>The data category within the source system (e.g., "IncidentReports").</summary>
    public string Category { get; }

    private DataSource(string systemName, string category)
    {
        SystemName = systemName;
        Category = category;
    }

    /// <summary>Creates a <see cref="DataSource"/> from system name and category.</summary>
    /// <exception cref="ArgumentException">Thrown when either argument is empty.</exception>
    public static DataSource From(string systemName, string category)
    {
        if (string.IsNullOrWhiteSpace(systemName))
            throw new ArgumentException("SystemName cannot be empty.", nameof(systemName));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty.", nameof(category));
        return new DataSource(systemName.Trim(), category.Trim());
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SystemName;
        yield return Category;
    }

    /// <inheritdoc />
    public override string ToString() => $"{SystemName}/{Category}";
}
