// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           IDataLakeActivityDomainEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Intelligence.DataLake domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Intelligence.DataLake.Events;

/// <summary>Marker interface for all Intelligence.DataLake domain events.</summary>
public interface IDataLakeActivityDomainEvent : IDomainEvent { }
