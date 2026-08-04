// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStream.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a characterized source of hazardous waste generation.
//                  Characterization data (EPA waste code, hazard class) is versioned, never
//                  overwritten in place -- re-characterization increments CharacterizationVersion
//                  and raises a new event; the prior version remains reconstructable from the
//                  event/history stream. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Events;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates;

/// <summary>Aggregate root for a characterized source of hazardous waste generation.</summary>
public sealed partial class WasteStream
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this WasteStream aggregate.</summary>
    public WasteStreamId WasteStreamId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public WasteStreamStatus Status { get; private set; }

    // ---- Characterization ---------------------------------------------------

    /// <summary>Human-readable name of the waste stream.</summary>
    public string StreamName { get; private set; } = string.Empty;

    /// <summary>EPA hazardous waste code (e.g. D001, F003) for the current characterization version.</summary>
    public string EpaWasteCode { get; private set; } = string.Empty;

    /// <summary>RCRA-style hazard classification for the current characterization version.</summary>
    public WasteHazardClass HazardClass { get; private set; }

    /// <summary>Generator's EPA identification number.</summary>
    public string GeneratorEpaId { get; private set; } = string.Empty;

    /// <summary>Monotonically increasing version of the characterization data. Re-characterization
    /// increments this value; it is never decremented or overwritten in place.</summary>
    public int CharacterizationVersion { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IWasteStreamDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWasteStreamDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWasteStreamDomainEvent>(_domainEvents);

    // For rehydration only.
    private WasteStream() { }

    // ---- Factory: Create (Characterize) -------------------------------------------

    /// <summary>Characterizes a new waste stream and raises <see cref="WasteStreamCharacterizedEvent"/>.</summary>
    public static WasteStream Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string streamName = "",
        string epaWasteCode = "",
        WasteHazardClass hazardClass = WasteHazardClass.NonHazardous,
        string generatorEpaId = "")
    {
        var agg = new WasteStream
        {
            WasteStreamId            = WasteStreamId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = WasteStreamStatus.Characterized,
            StreamName               = (streamName ?? string.Empty).Trim(),
            EpaWasteCode             = (epaWasteCode ?? string.Empty).Trim(),
            HazardClass              = hazardClass,
            GeneratorEpaId           = (generatorEpaId ?? string.Empty).Trim(),
            CharacterizationVersion  = 1,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new WasteStreamCharacterizedEvent(
            WasteStreamEventId.New(), agg.WasteStreamId, agg.EpaWasteCode, agg.HazardClass,
            agg.CharacterizationVersion, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Recharacterize -----------------------------------------------

    /// <summary>Re-characterizes the waste stream. Prior characterization data is retained as
    /// history via the version counter; nothing is overwritten in place.</summary>
    public void Recharacterize(
        string epaWasteCode, WasteHazardClass hazardClass,
        UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(WasteStreamStatus.Characterized);

        EpaWasteCode = (epaWasteCode ?? string.Empty).Trim();
        HazardClass = hazardClass;
        CharacterizationVersion++;

        AddDomainEvent(new WasteStreamRecharacterizedEvent(
            WasteStreamEventId.New(), WasteStreamId, EpaWasteCode, HazardClass,
            CharacterizationVersion, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Deactivate -----------------------------------------------------

    /// <summary>Deactivates the waste stream.</summary>
    public void Deactivate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(WasteStreamStatus.Characterized);

        Status = WasteStreamStatus.Deactivated;

        AddDomainEvent(new WasteStreamDeactivatedEvent(
            WasteStreamEventId.New(), WasteStreamId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IWasteStreamDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params WasteStreamStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for WasteStream {WasteStreamId}.");
    }
}
