// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Identity
//  Layer:          Domain
//  File:           IPartnerIdentityMappingDomainEvent.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PartnerIdentityMapping domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Identity.Events;

/// <summary>Marker interface for all PartnerIdentityMapping domain events.</summary>
public interface IPartnerIdentityMappingDomainEvent : IDomainEvent { }
