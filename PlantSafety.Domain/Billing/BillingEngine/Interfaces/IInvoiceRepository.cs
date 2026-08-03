// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           IInvoiceRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction for the Invoice aggregate. Implemented in the
//                  Infrastructure layer. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.Aggregates;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Billing.BillingEngine.Interfaces;

/// <summary>Repository for <see cref="Invoice"/> persistence.</summary>
public interface IInvoiceRepository
{
    /// <summary>Persists a newly generated invoice.</summary>
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing invoice (status transitions).</summary>
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single invoice by identity, or <see langword="null"/> if not found.</summary>
    Task<Invoice?> GetByIdAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the invoice already generated for the given tenant/period, or <see langword="null"/> if none exists yet.</summary>
    Task<Invoice?> GetByTenantAndPeriodAsync(TenantId tenantId, BillingPeriod period, CancellationToken cancellationToken = default);

    /// <summary>Lists every invoice generated for the given tenant, most recent period first.</summary>
    Task<IReadOnlyList<Invoice>> ListByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}
