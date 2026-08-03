// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           ITenantDomainEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all domain events raised by the Tenant aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Tenancy.Tenant.Events;

/// <summary>Marker interface for all domain events raised by the Tenant aggregate.</summary>
public interface ITenantDomainEvent : IDomainEvent { }
