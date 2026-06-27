// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics
//  Layer:          Domain
//  File:           IAnalyticsDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level Analytics domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Events;

/// <summary>Marker interface for top-level Analytics domain events.</summary>
public interface IAnalyticsDomainEvent : IDomainEvent { }
