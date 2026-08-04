// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           ICognitiveLoadAnalyticsDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all CognitiveLoadAnalytics domain events (CognitiveLoadLedger
//                  and CognitiveRiskPrediction aggregates share this event contract).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Events;

/// <summary>Marker interface for all CognitiveLoadAnalytics domain events.</summary>
public interface ICognitiveLoadAnalyticsDomainEvent : IDomainEvent { }
