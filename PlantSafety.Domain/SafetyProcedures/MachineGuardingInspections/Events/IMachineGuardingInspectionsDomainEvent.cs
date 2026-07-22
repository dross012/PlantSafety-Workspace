// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           IMachineGuardingInspectionsDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all MachineGuardingInspections domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Events;

/// <summary>Marker interface for all MachineGuardingInspections domain events.</summary>
public interface IMachineGuardingInspectionsDomainEvent : IDomainEvent { }