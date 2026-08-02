// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           IRootCauseAnalysisDomainEvent.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all RootCauseAnalysis domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Events;

/// <summary>Marker interface for all RootCauseAnalysis domain events.</summary>
public interface IRootCauseAnalysisDomainEvent : IDomainEvent { }
