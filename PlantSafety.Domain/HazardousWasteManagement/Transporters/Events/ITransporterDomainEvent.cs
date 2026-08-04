// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           ITransporterDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Marker interface for all Transporter domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Events;

/// <summary>Marker interface for all Transporter domain events.</summary>
public interface ITransporterDomainEvent : IDomainEvent { }
