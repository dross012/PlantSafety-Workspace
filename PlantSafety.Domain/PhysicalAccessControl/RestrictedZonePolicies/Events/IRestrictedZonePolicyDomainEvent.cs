// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           IRestrictedZonePolicyDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all RestrictedZonePolicy domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Events;

/// <summary>Marker interface for all RestrictedZonePolicy domain events.</summary>
public interface IRestrictedZonePolicyDomainEvent : IDomainEvent { }
