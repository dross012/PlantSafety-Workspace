// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           IWorkOrderPriorityDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all WorkOrderPriority domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Events;

/// <summary>Marker interface for all WorkOrderPriority domain events.</summary>
public interface IWorkOrderPriorityDomainEvent : IDomainEvent { }