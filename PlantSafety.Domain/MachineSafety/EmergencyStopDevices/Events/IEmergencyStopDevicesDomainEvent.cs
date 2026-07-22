// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           IEmergencyStopDevicesDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all EmergencyStopDevices domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Events;

/// <summary>Marker interface for all EmergencyStopDevices domain events.</summary>
public interface IEmergencyStopDevicesDomainEvent : IDomainEvent { }