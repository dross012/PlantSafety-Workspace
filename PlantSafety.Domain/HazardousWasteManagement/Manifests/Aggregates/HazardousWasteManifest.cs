// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifest.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for the RCRA-style Uniform Hazardous Waste Manifest --
//                  the regulatory chain-of-custody document tracking waste from generator
//                  through transporter to TSDF. Encodes two RCRA-style invariants as aggregate
//                  rules: (1) the manifest becomes immutable the instant the transporter signs
//                  for custody (IsImmutable / TransporterSignedAtUtc), and (2) records are
//                  retained for a minimum of 3 years from generation (RetentionExpiresAtUtc).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Enums;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Events;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Exceptions;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates;

/// <summary>Aggregate root for the RCRA-style Uniform Hazardous Waste Manifest.</summary>
public sealed partial class HazardousWasteManifest
{
    /// <summary>Minimum regulatory retention period, in years, for manifest records (data-governance invariant).</summary>
    public const int MinimumRetentionYears = 3;

    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this HazardousWasteManifest aggregate.</summary>
    public HazardousWasteManifestId HazardousWasteManifestId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazardousWasteManifestStatus Status { get; private set; }

    // ---- Manifest detail -----------------------------------------------------

    /// <summary>EPA Uniform Hazardous Waste Manifest tracking number.</summary>
    public string ManifestNumber { get; private set; } = string.Empty;

    /// <summary>Generator's EPA identification number.</summary>
    public string GeneratorEpaId { get; private set; } = string.Empty;

    /// <summary>Identity of the characterized waste stream being manifested.</summary>
    public Guid WasteStreamId { get; private set; }

    /// <summary>Identity of the assigned transporter.</summary>
    public Guid? TransporterId { get; private set; }

    /// <summary>Identity of the destination TSDF facility.</summary>
    public Guid? TsdfFacilityId { get; private set; }

    /// <summary>UTC timestamp the manifest was generated.</summary>
    public UtcTimestamp GeneratedAtUtc { get; private set; }

    /// <summary>UTC timestamp the transporter signed for custody. Once set, the manifest is immutable.</summary>
    public UtcTimestamp? TransporterSignedAtUtc { get; private set; }

    /// <summary>UTC timestamp the TSDF facility signed for receipt.</summary>
    public UtcTimestamp? TsdfReceivedAtUtc { get; private set; }

    /// <summary>UTC timestamp the manifest was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary><see langword="true"/> once the transporter has signed; blocks all further field mutation.</summary>
    public bool IsImmutable { get; private set; }

    /// <summary>Most recent discrepancy reason, if any.</summary>
    public string? DiscrepancyReason { get; private set; }

    /// <summary>UTC instant the mandatory 3-year regulatory retention period expires.</summary>
    public UtcTimestamp RetentionExpiresAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazardousWasteManifestDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazardousWasteManifestDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazardousWasteManifestDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazardousWasteManifest() { }

    // ---- Factory: Create (Generate) -----------------------------------------------------------

    /// <summary>Generates a new manifest and raises <see cref="ManifestGeneratedEvent"/>.</summary>
    public static HazardousWasteManifest Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string manifestNumber = "",
        string generatorEpaId = "",
        Guid wasteStreamId = default)
    {
        var agg = new HazardousWasteManifest
        {
            HazardousWasteManifestId = HazardousWasteManifestId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = HazardousWasteManifestStatus.Generated,
            ManifestNumber           = (manifestNumber ?? string.Empty).Trim(),
            GeneratorEpaId           = (generatorEpaId ?? string.Empty).Trim(),
            WasteStreamId            = wasteStreamId,
            GeneratedAtUtc           = createdAtUtc,
            IsImmutable              = false,
            RetentionExpiresAtUtc    = new UtcTimestamp(createdAtUtc.Value.AddYears(MinimumRetentionYears)),
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new ManifestGeneratedEvent(
            HazardousWasteManifestEventId.New(), agg.HazardousWasteManifestId, agg.ManifestNumber, wasteStreamId,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordTransporterPickup -----------------------------------------------

    /// <summary>Records the transporter's signature and custody transfer. From this point the
    /// manifest is immutable (RCRA chain-of-custody invariant).</summary>
    public void RecordTransporterPickup(
        Guid transporterId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardousWasteManifestStatus.Generated);

        TransporterId = transporterId;
        TransporterSignedAtUtc = occurredAtUtc;
        IsImmutable = true;
        Status = HazardousWasteManifestStatus.InTransit;

        AddDomainEvent(new TransporterPickupRecordedEvent(
            HazardousWasteManifestEventId.New(), HazardousWasteManifestId, transporterId,
            occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordTsdfReceipt -----------------------------------------------

    /// <summary>Records the TSDF facility's signature for receipt.</summary>
    public void RecordTsdfReceipt(
        Guid tsdfFacilityId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardousWasteManifestStatus.InTransit);
        EnsureNotDiscrepant();

        TsdfFacilityId = tsdfFacilityId;
        TsdfReceivedAtUtc = occurredAtUtc;
        Status = HazardousWasteManifestStatus.ReceivedAtTsdf;

        AddDomainEvent(new TsdfReceiptRecordedEvent(
            HazardousWasteManifestEventId.New(), HazardousWasteManifestId, tsdfFacilityId,
            occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes the manifest following disposal certification.</summary>
    public void Close(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardousWasteManifestStatus.ReceivedAtTsdf);

        Status = HazardousWasteManifestStatus.Closed;
        ClosedAtUtc = occurredAtUtc;

        AddDomainEvent(new ManifestClosedEvent(
            HazardousWasteManifestEventId.New(), HazardousWasteManifestId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: FlagDiscrepancy -----------------------------------------------

    /// <summary>Flags a discrepancy against the manifest (quantity, waste code, or signature mismatch).
    /// Discrepancy flagging is a metadata annotation only -- it does not mutate the immutable,
    /// transporter-signed core manifest fields.</summary>
    public void FlagDiscrepancy(
        string discrepancyReason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (string.IsNullOrWhiteSpace(discrepancyReason))
            throw new ArgumentException("Discrepancy reason must be provided.", nameof(discrepancyReason));

        DiscrepancyReason = discrepancyReason.Trim();
        Status = HazardousWasteManifestStatus.DiscrepancyFlagged;

        AddDomainEvent(new ManifestDiscrepancyFlaggedEvent(
            HazardousWasteManifestEventId.New(), HazardousWasteManifestId, DiscrepancyReason,
            occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazardousWasteManifestDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params HazardousWasteManifestStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for HazardousWasteManifest {HazardousWasteManifestId}.");
    }

    private void EnsureNotDiscrepant()
    {
        if (Status == HazardousWasteManifestStatus.DiscrepancyFlagged)
            throw new InvalidOperationException(
                $"HazardousWasteManifest {HazardousWasteManifestId} has an open discrepancy and must be resolved first.");
    }
}
