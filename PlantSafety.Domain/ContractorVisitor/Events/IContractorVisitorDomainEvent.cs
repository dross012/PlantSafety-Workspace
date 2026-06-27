// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           IContractorVisitorDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ContractorVisitor domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Events;

/// <summary>Marker interface for all ContractorVisitor domain events.</summary>
public interface IContractorVisitorDomainEvent : IDomainEvent { }
