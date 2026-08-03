// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           InvoiceEvents.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised by the Invoice aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Billing.BillingEngine.Events;

/// <summary>Raised when a new Invoice has been generated (Draft status) for a tenant's billing period.</summary>
public sealed record InvoiceGeneratedEvent(
    Guid EventId, InvoiceId InvoiceId, TenantId TenantId, decimal TotalDueAmount, string Currency, UtcTimestamp OccurredAtUtc) : IBillingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "InvoiceGenerated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a Draft Invoice is finalized and issued to the tenant.</summary>
public sealed record InvoiceIssuedEvent(Guid EventId, InvoiceId InvoiceId, TenantId TenantId, UtcTimestamp OccurredAtUtc) : IBillingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "InvoiceIssued";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an Invoice is successfully charged/paid in full.</summary>
public sealed record InvoicePaidEvent(
    Guid EventId, InvoiceId InvoiceId, TenantId TenantId, string PaymentProcessorTransactionId, UtcTimestamp OccurredAtUtc) : IBillingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "InvoicePaid";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a charge attempt against an Invoice fails.</summary>
public sealed record InvoicePaymentFailedEvent(
    Guid EventId, InvoiceId InvoiceId, TenantId TenantId, string FailureReason, UtcTimestamp OccurredAtUtc) : IBillingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "InvoicePaymentFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an Invoice is voided/cancelled before payment.</summary>
public sealed record InvoiceVoidedEvent(Guid EventId, InvoiceId InvoiceId, TenantId TenantId, string Reason, UtcTimestamp OccurredAtUtc) : IBillingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "InvoiceVoided";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
