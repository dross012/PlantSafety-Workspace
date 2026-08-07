// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           IPackagingSpecificationDomainEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PackagingSpecification domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Events;

/// <summary>Marker interface for all PackagingSpecification domain events.</summary>
public interface IPackagingSpecificationDomainEvent : IDomainEvent { }
