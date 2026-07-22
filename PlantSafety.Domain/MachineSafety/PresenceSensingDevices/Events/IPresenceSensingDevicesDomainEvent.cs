// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           IPresenceSensingDevicesDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PresenceSensingDevices domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Events;

/// <summary>Marker interface for all PresenceSensingDevices domain events.</summary>
public interface IPresenceSensingDevicesDomainEvent : IDomainEvent { }