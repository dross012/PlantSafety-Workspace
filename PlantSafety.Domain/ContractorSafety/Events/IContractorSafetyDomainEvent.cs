// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety
//  Layer:          Domain
//  File:           IContractorSafetyDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level ContractorSafety domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.Events;

/// <summary>Marker interface for top-level ContractorSafety domain events.</summary>
public interface IContractorSafetyDomainEvent : IDomainEvent { }