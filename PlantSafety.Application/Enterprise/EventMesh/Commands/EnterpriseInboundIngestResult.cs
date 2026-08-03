// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.EventMesh
//  Layer:          Application
//  File:           EnterpriseInboundIngestResult.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Result payload for <see cref="IngestEnterpriseInboundEventCommand"/>. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Enterprise.EventMesh.Commands;

/// <summary>Outcome of ingesting one inbound <c>EnterpriseEventEnvelope</c> from an external system.</summary>
/// <param name="EventId">The ingested envelope's EventId.</param>
/// <param name="EventType">The ingested envelope's EventType.</param>
/// <param name="Handled">
/// <see langword="true"/> if a registered <c>IEnterpriseInboundEventTranslator</c> handled this
/// EventType; <see langword="false"/> if no translator recognises it (gracefully ignored, not an
/// error, matching this platform's additive-only-evolution convention -- a future EventType this
/// version of the platform does not yet understand must not fail the inbound adapter).
/// </param>
public sealed record EnterpriseInboundIngestResult(Guid EventId, string EventType, bool Handled);
