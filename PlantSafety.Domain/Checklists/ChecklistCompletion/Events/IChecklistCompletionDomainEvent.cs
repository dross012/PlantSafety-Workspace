// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           IChecklistCompletionDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ChecklistCompletion domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Events;

/// <summary>Marker interface for all ChecklistCompletion domain events.</summary>
public interface IChecklistCompletionDomainEvent : IDomainEvent { }