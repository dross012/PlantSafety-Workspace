// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the DisposalCertificate lifecycle: receipt and
//                  verification.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Events;

/// <summary>Raised when a disposal certificate is received from a TSDF facility.</summary>
public sealed record DisposalCertificateReceivedEvent(
    DisposalCertificateEventId EventId,
    DisposalCertificateId DisposalCertificateId,
    Guid ManifestId,
    Guid TsdfFacilityId,
    DisposalMethod DisposalMethod,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDisposalCertificateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DisposalCertificateReceived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a disposal certificate is verified against its manifest.</summary>
public sealed record DisposalCertificateVerifiedEvent(
    DisposalCertificateEventId EventId,
    DisposalCertificateId DisposalCertificateId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDisposalCertificateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DisposalCertificateVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
