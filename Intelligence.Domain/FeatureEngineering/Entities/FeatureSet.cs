/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FeatureSet.cs
    DESCRIPTION:        Entity: a named, versioned set of ML features. Immutable once published.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.FeatureEngineering.ValueObjects;

namespace Intelligence.Domain.FeatureEngineering.Entities;

/// <summary>A named, versioned collection of ML features published to the Intelligence feature store.</summary>
public sealed class FeatureSet : Entity<Guid>
{
    private readonly List<FeatureName> _features = new();

    /// <summary>The name of this feature set (e.g., "IncidentRiskFeatures").</summary>
    public string Name { get; private init; }

    /// <summary>The version of this feature set.</summary>
    public FeatureVersion Version { get; private init; }

    /// <summary>The masked data source path used to compute these features.</summary>
    public string DataSourcePath { get; private init; }

    /// <summary>Whether this feature set has been published to the feature store.</summary>
    public bool IsPublished { get; private set; }

    /// <summary>UTC timestamp when this feature set was computed.</summary>
    public DateTime ComputedAtUtc { get; private init; }

    /// <summary>UTC timestamp when this feature set was published. Null until published.</summary>
    public DateTime? PublishedAtUtc { get; private set; }

    /// <summary>Feature names in this set. Immutable once published.</summary>
    public IReadOnlyList<FeatureName> Features => _features.AsReadOnly();

    private FeatureSet(Guid id, string name, FeatureVersion version, string dataSourcePath) : base(id)
    {
        Name = name;
        Version = version;
        DataSourcePath = dataSourcePath;
        ComputedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Creates a new unpublished <see cref="FeatureSet"/>.</summary>
    public static FeatureSet Create(string name, FeatureVersion version, string dataSourcePath)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(dataSourcePath)) throw new ArgumentException("DataSourcePath cannot be empty.", nameof(dataSourcePath));
        return new FeatureSet(Guid.NewGuid(), name.Trim(), version, dataSourcePath.Trim());
    }

    /// <summary>Adds a feature. Only permitted before publication.</summary>
    /// <exception cref="InvalidOperationException">Thrown when already published.</exception>
    public void AddFeature(FeatureName featureName)
    {
        ArgumentNullException.ThrowIfNull(featureName);
        if (IsPublished) throw new InvalidOperationException($"Cannot add features to published FeatureSet '{Name}' v{Version}.");
        if (!_features.Contains(featureName)) _features.Add(featureName);
    }

    /// <summary>Marks this feature set as published. Immutable thereafter.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no features are present or already published.</exception>
    public void Publish()
    {
        if (IsPublished) throw new InvalidOperationException($"FeatureSet '{Name}' v{Version} is already published.");
        if (_features.Count == 0) throw new InvalidOperationException($"FeatureSet '{Name}' must contain at least one feature.");
        IsPublished = true;
        PublishedAtUtc = DateTime.UtcNow;
    }
}
