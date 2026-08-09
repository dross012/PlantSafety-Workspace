// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Twins
//  File:           DigitalTwinEvent.cs
//  Created:        2026-08-08
//  Description:    Phase D.7: a read-only virtual snapshot of a correlated
//                  real domain event, for simulation purposes only.
// ============================================================================
namespace Enterprise.DigitalTwin.Abstractions.Twins;

/// <summary>A read-only virtual snapshot of a correlated real domain event.</summary>
public sealed record DigitalTwinEvent(
    Guid EventId,
    string EventType,
    DateTime OccurredAtUtc);
