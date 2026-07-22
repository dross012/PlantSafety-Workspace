// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           IChecklistTemplateDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ChecklistTemplate domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Events;

/// <summary>Marker interface for all ChecklistTemplate domain events.</summary>
public interface IChecklistTemplateDomainEvent : IDomainEvent { }