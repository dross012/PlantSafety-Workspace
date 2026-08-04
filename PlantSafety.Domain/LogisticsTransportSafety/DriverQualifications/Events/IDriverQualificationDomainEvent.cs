// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           IDriverQualificationDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all DriverQualification domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Events;

/// <summary>Marker interface for all DriverQualification domain events.</summary>
public interface IDriverQualificationDomainEvent : IDomainEvent { }
