// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance
//  Layer:          Domain
//  File:           IComplianceDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level Compliance domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Events;

/// <summary>Marker interface for top-level Compliance domain events.</summary>
public interface IComplianceDomainEvent : IDomainEvent { }
