// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           IBillingDomainEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for Billing.BillingEngine domain events, mirroring the
//                  ITenantDomainEvent / IOptimizationRunDomainEvent convention used elsewhere in
//                  this codebase. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Billing.BillingEngine.Events;

/// <summary>Marker interface for all Billing.BillingEngine domain events.</summary>
public interface IBillingDomainEvent : IDomainEvent
{
    /// <summary>Unique identifier of this event instance.</summary>
    Guid EventId { get; }
}
