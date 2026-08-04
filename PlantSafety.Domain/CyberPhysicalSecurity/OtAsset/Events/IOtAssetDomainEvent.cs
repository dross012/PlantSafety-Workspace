// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           IOtAssetDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all OtAsset domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Events;

/// <summary>Marker interface for all OtAsset domain events.</summary>
public interface IOtAssetDomainEvent : IDomainEvent
{
    /// <summary>Identity of the event instance.</summary>
    OtAssetEventId EventId { get; }
}
