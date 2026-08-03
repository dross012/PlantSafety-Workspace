// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Domain
//  File:           IUsageMeteringSnapshotRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction for persisting/retrieving UsageMeteringSnapshot
//                  aggregates. Implemented in the Infrastructure layer. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.Metering.Aggregates;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Billing.Metering.Interfaces;

/// <summary>Repository for <see cref="UsageMeteringSnapshot"/> persistence.</summary>
public interface IUsageMeteringSnapshotRepository
{
    /// <summary>Persists a newly captured snapshot (upsert -- re-capturing the same tenant/period replaces the prior snapshot).</summary>
    Task SaveAsync(UsageMeteringSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the snapshot for the given tenant/period, or <see langword="null"/> if none has been captured yet.</summary>
    Task<UsageMeteringSnapshot?> GetAsync(TenantId tenantId, BillingPeriod period, CancellationToken cancellationToken = default);
}
