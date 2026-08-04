// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           ControlChangeEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ControlChangeEvent bounded context (Anomalous
//                  Control-Change Detection subdomain of CyberPhysicalSecurity). Represents a
//                  single observed change to an OT/ICS control-system parameter, ingested from
//                  SCADA/HMI/ICS telemetry. Append-only, hash-chained (RecordHash derived from
//                  PriorRecordHash) per the module's tamper-evident data-governance requirement --
//                  raw OT control-change records must never be mutated after creation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Exceptions;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Aggregates;

/// <summary>Aggregate root for the ControlChangeEvent bounded context. Immutable once recorded,
/// aside from its <see cref="AuthorizationStatus"/> classification.</summary>
public sealed partial class ControlChangeEvent
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this ControlChangeEvent aggregate.</summary>
    public ControlChangeEventId ControlChangeEventId { get; private set; }

    /// <summary>Tenant that owns this control-change record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the asset is located at.</summary>
    public Guid SiteId { get; private set; }

    /// <summary>Identity of the OT asset the change was observed on.</summary>
    public OtAssetId OtAssetId { get; private set; }

    // ---- Change details --------------------------------------------------------

    /// <summary>Human-readable description of the change.</summary>
    public string ChangeDescription { get; private set; } = string.Empty;

    /// <summary>Name of the control-system parameter that changed (e.g., a setpoint tag).</summary>
    public string ChangedParameter { get; private set; } = string.Empty;

    /// <summary>Prior value of the parameter, as text.</summary>
    public string OldValue { get; private set; } = string.Empty;

    /// <summary>New value of the parameter, as text.</summary>
    public string NewValue { get; private set; } = string.Empty;

    /// <summary>Identity of the user who initiated the change, if attributable. Null if unknown/unattributed.</summary>
    public UserId? ChangeSourceUserId { get; private set; }

    /// <summary>Authorization classification of this change.</summary>
    public ControlChangeAuthorizationStatus AuthorizationStatus { get; private set; }

    /// <summary>UTC timestamp the change was observed on the OT network (telemetry timestamp).</summary>
    public UtcTimestamp DetectedAtUtc { get; private set; }

    /// <summary>Hash of the immediately preceding record in this asset's change-history chain.
    /// Null for the first record. Supports tamper-evident, append-only OT control-change logs.</summary>
    public string? PriorRecordHash { get; private set; }

    /// <summary>SHA-256 hash of this record's canonical content, chained from <see cref="PriorRecordHash"/>.</summary>
    public string RecordHash { get; private set; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that recorded this event (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IControlChangeEventDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IControlChangeEventDomainEvent> DomainEvents
        => new ReadOnlyCollection<IControlChangeEventDomainEvent>(_domainEvents);

    // For rehydration only.
    private ControlChangeEvent() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Records a new control-change observation and raises <see cref="ControlChangeEventRecordedEvent"/>.</summary>
    public static ControlChangeEvent Record(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        OtAssetId otAssetId,
        string changeDescription,
        string changedParameter,
        string oldValue,
        string newValue,
        UtcTimestamp detectedAtUtc,
        string? priorRecordHash,
        UserId? changeSourceUserId = null,
        ControlChangeAuthorizationStatus authorizationStatus = ControlChangeAuthorizationStatus.PendingReview)
    {
        var agg = new ControlChangeEvent
        {
            ControlChangeEventId = ControlChangeEventId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            OtAssetId             = otAssetId,
            ChangeDescription     = (changeDescription ?? string.Empty).Trim(),
            ChangedParameter      = (changedParameter ?? string.Empty).Trim(),
            OldValue              = oldValue ?? string.Empty,
            NewValue              = newValue ?? string.Empty,
            ChangeSourceUserId    = changeSourceUserId,
            AuthorizationStatus   = authorizationStatus,
            DetectedAtUtc         = detectedAtUtc,
            PriorRecordHash       = priorRecordHash,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.RecordHash = ComputeRecordHash(agg, priorRecordHash);

        agg.AddDomainEvent(new ControlChangeEventRecordedEvent(
            EventId:               ControlChangeDomainEventId.New(),
            ControlChangeEventId:  agg.ControlChangeEventId,
            OtAssetId:             agg.OtAssetId,
            RecordedAtUtc:         createdAtUtc,
            Actor:                 actor,
            CorrelationId:         correlationId,
            CausationId:           causationId));

        if (authorizationStatus == ControlChangeAuthorizationStatus.Unauthorized)
        {
            agg.AddDomainEvent(new UnauthorizedControlChangeDetectedEvent(
                EventId:              ControlChangeDomainEventId.New(),
                ControlChangeEventId: agg.ControlChangeEventId,
                OtAssetId:            agg.OtAssetId,
                ChangedParameter:     agg.ChangedParameter,
                DetectedAtUtc:        detectedAtUtc,
                Actor:                actor,
                CorrelationId:        correlationId,
                CausationId:          causationId));
        }

        return agg;
    }

    // ---- Behavior ------------------------------------------------------------

    /// <summary>Reclassifies the authorization status (e.g., after a change-management review),
    /// raising <see cref="UnauthorizedControlChangeDetectedEvent"/> when the reclassification
    /// results in an Unauthorized determination.</summary>
    public void FlagUnauthorized(UtcTimestamp detectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (AuthorizationStatus == ControlChangeAuthorizationStatus.Unauthorized)
            return;

        AuthorizationStatus = ControlChangeAuthorizationStatus.Unauthorized;

        AddDomainEvent(new UnauthorizedControlChangeDetectedEvent(
            EventId:              ControlChangeDomainEventId.New(),
            ControlChangeEventId: ControlChangeEventId,
            OtAssetId:            OtAssetId,
            ChangedParameter:     ChangedParameter,
            DetectedAtUtc:        detectedAtUtc,
            Actor:                actor,
            CorrelationId:        correlationId,
            CausationId:          causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IControlChangeEventDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Computes the tamper-evident hash for this record by combining the prior record's hash with
    /// this record's canonical content, so any retroactive edit of a historical record breaks the
    /// chain for every subsequent record on the same asset.
    /// </summary>
    private static string ComputeRecordHash(ControlChangeEvent record, string? priorRecordHash)
    {
        var canonical = string.Join('|',
            priorRecordHash ?? string.Empty,
            record.ControlChangeEventId.Value,
            record.OtAssetId.Value,
            record.ChangedParameter,
            record.OldValue,
            record.NewValue,
            record.DetectedAtUtc.Value.ToString("O"));

        var bytes = System.Text.Encoding.UTF8.GetBytes(canonical);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
