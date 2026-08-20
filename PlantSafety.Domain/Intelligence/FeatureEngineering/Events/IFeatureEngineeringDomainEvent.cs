// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           IFeatureEngineeringDomainEvent.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Intelligence.FeatureEngineering domain events. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Events.IDataLakeActivityDomainEvent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Events;

/// <summary>Marker interface for all Intelligence.FeatureEngineering domain events.</summary>
public interface IFeatureEngineeringDomainEvent : IDomainEvent { }
