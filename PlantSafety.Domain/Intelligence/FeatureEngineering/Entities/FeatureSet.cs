// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureSet.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Entity for the Intelligence.FeatureEngineering bounded context: a named,
//                  versioned set of ML features, immutable once published. Ported (rebuilt, not
//                  copy-pasted) from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/Entities/FeatureSet.cs -- which this
//                  whole subtree (Intelligence.Domain) is a confirmed-dead, unwired project slated
//                  for later deletion -- into this real, live Intelligence module's own
//                  conventions: strong-typed FeatureSetId (not a plain Guid Id on
//                  Entity&lt;Guid&gt;), tenant/site scoping, guarded-mutation/domain-event/exception
//                  pattern, and PlantSafety.Domain.Intelligence.FeatureEngineering.* namespace.
//                  Mirrors PlantSafety.Domain.Intelligence.DataLake.Aggregates.DataLakeActivity's
//                  structural precedent, adapted for an owned Entity rather than an aggregate root
//                  per the orphan's original documented design (FeatureSet is owned by the Feature
//                  Engineering subsystem and published immutably; see
//                  Interfaces.IFeatureSetRepository's remarks).
//                  Preserves the orphan's feature-set versioning (FeatureVersion.Supersedes) and
//                  publish-readiness (Services.FeatureEngineeringDomainService.IsReadyToPublish)
//                  business rules.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Events;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Exceptions;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Rules;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Entities;

/// <summary>A named, versioned collection of ML features published to the Intelligence feature store.</summary>
public sealed class FeatureSet
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this FeatureSet entity.</summary>
    public FeatureSetId FeatureSetId { get; private set; }

    /// <summary>Tenant that owns this feature set.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    // ---- FeatureSet-specific business properties -------------------------------

    /// <summary>The name of this feature set (e.g., "IncidentRiskFeatures"). Normalized, non-empty.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The version of this feature set.</summary>
    public FeatureVersion Version { get; private set; } = null!;

    /// <summary>The masked data source path used to compute these features.</summary>
    public string DataSourcePath { get; private set; } = string.Empty;

    /// <summary>Whether this feature set has been published to the feature store. Immutable once <see langword="true"/>.</summary>
    public bool IsPublished { get; private set; }

    /// <summary>UTC timestamp when this feature set was computed.</summary>
    public UtcTimestamp ComputedAtUtc { get; private set; }

    /// <summary>UTC timestamp when this feature set was published. Null until published.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    private readonly List<FeatureName> _features = [];

    /// <summary>Feature names in this set. Immutable once published.</summary>
    public IReadOnlyList<FeatureName> Features => _features.AsReadOnly();

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user or process that created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IFeatureEngineeringDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IFeatureEngineeringDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // For rehydration only.
    private FeatureSet() { }

    // ---- Factory ---------------------------------------------------------------

    /// <summary>
    /// Creates a new, unpublished <see cref="FeatureSet"/> and raises
    /// <see cref="FeatureSetCreatedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static FeatureSet Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string name,
        FeatureVersion version,
        string dataSourcePath)
    {
        ArgumentNullException.ThrowIfNull(version);

        var normalizedName = (name ?? string.Empty).Trim();
        var normalizedDataSourcePath = (dataSourcePath ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(normalizedName))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(normalizedDataSourcePath))
            throw new ArgumentException("DataSourcePath cannot be empty.", nameof(dataSourcePath));

        var featureSet = new FeatureSet
        {
            FeatureSetId = FeatureSetId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Name = normalizedName,
            Version = version,
            DataSourcePath = normalizedDataSourcePath,
            IsPublished = false,
            ComputedAtUtc = createdAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        featureSet.AddDomainEvent(new FeatureSetCreatedEvent(
            EventId: FeatureSetEventId.New(),
            FeatureSetId: featureSet.FeatureSetId,
            TenantId: tenantId,
            SiteId: siteId,
            Name: featureSet.Name,
            Version: version.Value,
            DataSourcePath: featureSet.DataSourcePath,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return featureSet;
    }

    // ---- Mutation: AddFeature ---------------------------------------------------

    /// <summary>Adds a feature. Only permitted before publication.</summary>
    /// <exception cref="FeatureSetAlreadyPublishedException">Thrown when already published.</exception>
    public void AddFeature(FeatureName featureName, CorrelationId correlationId, CausationId causationId)
    {
        ArgumentNullException.ThrowIfNull(featureName);
        if (!FeatureSetPublishRules.CanAddFeature(IsPublished))
            throw new FeatureSetAlreadyPublishedException(FeatureSetId, Name, Version.Value, correlationId, causationId);

        if (!_features.Contains(featureName)) _features.Add(featureName);
    }

    // ---- Lifecycle: Publish -----------------------------------------------------

    /// <summary>Marks this feature set as published. Immutable thereafter. Raises <see cref="FeatureSetPublishedEvent"/>.</summary>
    /// <exception cref="FeatureSetAlreadyPublishedException">Thrown when already published.</exception>
    /// <exception cref="FeatureSetEmptyException">Thrown when no features are present.</exception>
    public void Publish(UtcTimestamp publishedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (IsPublished)
            throw new FeatureSetAlreadyPublishedException(FeatureSetId, Name, Version.Value, correlationId, causationId);
        if (!FeatureSetPublishRules.CanPublish(IsPublished, _features.Count))
            throw new FeatureSetEmptyException(FeatureSetId, Name, Version.Value, correlationId, causationId);

        IsPublished = true;
        PublishedAtUtc = publishedAtUtc;

        AddDomainEvent(new FeatureSetPublishedEvent(
            EventId: FeatureSetEventId.New(),
            FeatureSetId: FeatureSetId,
            Name: Name,
            Version: Version.Value,
            FeatureCount: _features.Count,
            PublishedAtUtc: publishedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IFeatureEngineeringDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
