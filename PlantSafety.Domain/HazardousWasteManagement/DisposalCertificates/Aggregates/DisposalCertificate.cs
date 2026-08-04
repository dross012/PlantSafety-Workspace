// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificate.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a certificate of destruction/disposal received from a TSDF
//                  facility, closing the regulatory chain-of-custody loop for a manifest. Tracks
//                  turnaround time (manifest close to certificate receipt) for the "disposal
//                  certificate turnaround" KPI.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Events;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates;

/// <summary>Aggregate root for a hazardous-waste disposal certificate.</summary>
public sealed partial class DisposalCertificate
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this DisposalCertificate aggregate.</summary>
    public DisposalCertificateId DisposalCertificateId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DisposalCertificateStatus Status { get; private set; }

    // ---- Certificate detail -----------------------------------------------------

    /// <summary>Identity of the closing HazardousWasteManifest.</summary>
    public Guid ManifestId { get; private set; }

    /// <summary>Identity of the TSDF facility that issued the certificate.</summary>
    public Guid TsdfFacilityId { get; private set; }

    /// <summary>Method by which the waste was disposed of.</summary>
    public DisposalMethod DisposalMethod { get; private set; }

    /// <summary>UTC date the physical disposal occurred, as reported by the TSDF.</summary>
    public UtcTimestamp DisposalDateUtc { get; private set; }

    /// <summary>UTC timestamp this certificate was received by the generator.</summary>
    public UtcTimestamp CertificateReceivedAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IDisposalCertificateDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDisposalCertificateDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDisposalCertificateDomainEvent>(_domainEvents);

    // For rehydration only.
    private DisposalCertificate() { }

    // ---- Factory: Create (Record) -----------------------------------------------------------

    /// <summary>Records a new disposal certificate and raises <see cref="DisposalCertificateReceivedEvent"/>.</summary>
    public static DisposalCertificate Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid manifestId = default,
        Guid tsdfFacilityId = default,
        DisposalMethod disposalMethod = DisposalMethod.Landfill,
        UtcTimestamp disposalDateUtc = default)
    {
        var agg = new DisposalCertificate
        {
            DisposalCertificateId     = DisposalCertificateId.New(),
            TenantId                  = tenantId,
            SiteId                    = siteId,
            Status                    = DisposalCertificateStatus.Received,
            ManifestId                = manifestId,
            TsdfFacilityId            = tsdfFacilityId,
            DisposalMethod            = disposalMethod,
            DisposalDateUtc           = disposalDateUtc,
            CertificateReceivedAtUtc  = createdAtUtc,
            CreatedAtUtc              = createdAtUtc,
            CreatedBy                 = createdBy,
        };

        agg.AddDomainEvent(new DisposalCertificateReceivedEvent(
            DisposalCertificateEventId.New(), agg.DisposalCertificateId, manifestId, tsdfFacilityId,
            disposalMethod, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Verify -----------------------------------------------------------

    /// <summary>Verifies the disposal certificate against its source manifest.</summary>
    public void Verify(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(DisposalCertificateStatus.Received);

        Status = DisposalCertificateStatus.Verified;

        AddDomainEvent(new DisposalCertificateVerifiedEvent(
            DisposalCertificateEventId.New(), DisposalCertificateId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IDisposalCertificateDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params DisposalCertificateStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for DisposalCertificate {DisposalCertificateId}.");
    }
}
