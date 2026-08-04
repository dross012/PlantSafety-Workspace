// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAsset.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the OtAsset bounded context (OT Asset Inventory subdomain of
//                  CyberPhysicalSecurity). Represents a single OT/ICS asset (PLC, HMI, SCADA,
//                  sensor, actuator, etc.) tracked for cyber-physical security monitoring.
//                  Grounded in project_plantsafety_domains.md CyberPhysicalSecurity module spec.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Exceptions;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Aggregates;

/// <summary>Aggregate root for the OtAsset bounded context.</summary>
public sealed partial class OtAsset
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this OtAsset aggregate.</summary>
    public OtAssetId OtAssetId { get; private set; }

    /// <summary>Tenant that owns this OT asset record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this asset is physically located at.</summary>
    public Guid SiteId { get; private set; }

    // ---- Asset attributes ------------------------------------------------------

    /// <summary>Unique human-readable asset tag/label used on the plant floor.</summary>
    public string AssetTag { get; private set; } = string.Empty;

    /// <summary>Descriptive name of the asset.</summary>
    public string AssetName { get; private set; } = string.Empty;

    /// <summary>Classification of the OT/ICS asset.</summary>
    public OtAssetType AssetType { get; private set; }

    /// <summary>Vendor/manufacturer name.</summary>
    public string Vendor { get; private set; } = string.Empty;

    /// <summary>Model/part number.</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>Currently installed firmware/software version.</summary>
    public string FirmwareVersion { get; private set; } = string.Empty;

    /// <summary>OT network zone / Purdue Model level the asset resides in (e.g., "Level 1").</summary>
    public string NetworkZone { get; private set; } = string.Empty;

    /// <summary>Safety/production criticality rating.</summary>
    public OtAssetCriticality Criticality { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public OtAssetStatus Status { get; private set; }

    /// <summary>UTC timestamp this asset was last reconciled against the periodic inventory sweep.</summary>
    public UtcTimestamp? LastReconciledAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IOtAssetDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IOtAssetDomainEvent> DomainEvents
        => new ReadOnlyCollection<IOtAssetDomainEvent>(_domainEvents);

    // For rehydration only.
    private OtAsset() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Registers a new OtAsset and raises <see cref="OtAssetRegisteredEvent"/>.</summary>
    public static OtAsset Register(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        string assetTag,
        string assetName,
        OtAssetType assetType,
        string vendor = "",
        string model = "",
        string firmwareVersion = "",
        string networkZone = "",
        OtAssetCriticality criticality = OtAssetCriticality.Medium)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
            throw new OtAssetDomainException("AssetTag is required to register an OtAsset.");

        var agg = new OtAsset
        {
            OtAssetId       = OtAssetId.New(),
            TenantId        = tenantId,
            SiteId          = siteId,
            AssetTag        = assetTag.Trim(),
            AssetName       = (assetName ?? string.Empty).Trim(),
            AssetType       = assetType,
            Vendor          = (vendor ?? string.Empty).Trim(),
            Model           = (model ?? string.Empty).Trim(),
            FirmwareVersion = (firmwareVersion ?? string.Empty).Trim(),
            NetworkZone     = (networkZone ?? string.Empty).Trim(),
            Criticality     = criticality,
            Status          = OtAssetStatus.Active,
            CreatedAtUtc    = createdAtUtc,
            CreatedBy       = createdBy,
        };

        agg.AddDomainEvent(new OtAssetRegisteredEvent(
            EventId:         OtAssetEventId.New(),
            OtAssetId:       agg.OtAssetId,
            RegisteredAtUtc: createdAtUtc,
            Actor:           actor,
            CorrelationId:   correlationId,
            CausationId:     causationId));

        return agg;
    }

    // ---- Behavior ------------------------------------------------------------

    /// <summary>Marks this asset as reconciled during a periodic OT asset inventory sweep.</summary>
    public void Reconcile(UtcTimestamp reconciledAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        LastReconciledAtUtc = reconciledAtUtc;

        AddDomainEvent(new OtAssetReconciledEvent(
            EventId:        OtAssetEventId.New(),
            OtAssetId:      OtAssetId,
            ReconciledAtUtc: reconciledAtUtc,
            Actor:          actor,
            CorrelationId:  correlationId,
            CausationId:    causationId));
    }

    /// <summary>Isolates this asset from the OT network pending investigation.</summary>
    public void Quarantine(string reason, UtcTimestamp quarantinedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(OtAssetStatus.Active, OtAssetStatus.UnderReview);

        Status = OtAssetStatus.Quarantined;

        AddDomainEvent(new OtAssetQuarantinedEvent(
            EventId:          OtAssetEventId.New(),
            OtAssetId:        OtAssetId,
            Reason:           reason,
            QuarantinedAtUtc: quarantinedAtUtc,
            Actor:            actor,
            CorrelationId:    correlationId,
            CausationId:      causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IOtAssetDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params OtAssetStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new OtAssetInvalidStatusTransitionException(Status, allowed.Length > 0 ? allowed[0] : Status);
    }
}
