// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           Invoice.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single tenant's monthly invoice: base plan charge, usage
//                  overage line items (computed from §3.1 UsageMeteringSnapshot data against the
//                  tenant's SubscriptionPlan limits), and any applied discounts/credits. Generated
//                  by InvoiceFactory (which composes InvoiceCalculationRules + UsageOverageCalculator);
//                  this aggregate itself only owns lifecycle state transitions (Draft -> Issued ->
//                  Paid/PaymentFailed, or -> Void) and re-exposes the already-computed totals --
//                  it does not recompute money math itself, keeping a single source of truth for
//                  the calculation logic. Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.Domain.Billing.BillingEngine.Enums;
using PlantSafety.Domain.Billing.BillingEngine.Events;
using PlantSafety.Domain.Billing.BillingEngine.Exceptions;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Billing.BillingEngine.Aggregates;

/// <summary>Aggregate root for a single tenant's monthly invoice.</summary>
public sealed class Invoice
{
    private readonly List<InvoiceLineItem> _lineItems = [];
    private readonly List<IBillingDomainEvent> _domainEvents = [];

    /// <summary>Strong-typed identity of this invoice.</summary>
    public InvoiceId InvoiceId { get; private set; }

    /// <summary>The tenant this invoice was generated for.</summary>
    public TenantId TenantId { get; private set; }

    /// <summary>The billing period (calendar month) this invoice covers.</summary>
    public BillingPeriod Period { get; private set; }

    /// <summary>Every charge/discount/credit line item on this invoice, in the order they were computed.</summary>
    public IReadOnlyList<InvoiceLineItem> LineItems => new ReadOnlyCollection<InvoiceLineItem>(_lineItems);

    /// <summary>Pre-discount subtotal (sum of BasePlan + Overage line item amounts).</summary>
    public Money Subtotal { get; private set; }

    /// <summary>Final amount due after every discount/credit has been applied (never negative).</summary>
    public Money TotalDue { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Default;

    /// <summary>UTC instant this invoice was generated (Draft).</summary>
    public UtcTimestamp GeneratedAtUtc { get; private set; }

    /// <summary>UTC instant this invoice was issued to the tenant, if issued.</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>UTC instant this invoice was paid in full, if paid.</summary>
    public UtcTimestamp? PaidAtUtc { get; private set; }

    /// <summary>The payment processor's transaction identifier for the successful charge, if paid.</summary>
    public string? PaymentProcessorTransactionId { get; private set; }

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IBillingDomainEvent> DomainEvents => new ReadOnlyCollection<IBillingDomainEvent>(_domainEvents);

    // For rehydration only.
    private Invoice() { }

    /// <summary>
    /// Generates a new Draft invoice from already-computed line items and totals (see
    /// InvoiceFactory, which composes InvoiceCalculationRules for this computation). This
    /// constructor performs no money math of its own -- it only validates internal consistency
    /// (TotalDue must equal Subtotal plus every line item's Amount) before accepting the result.
    /// </summary>
    public static Invoice Generate(
        TenantId tenantId, BillingPeriod period, IReadOnlyList<InvoiceLineItem> lineItems,
        Money subtotal, Money totalDue, UtcTimestamp generatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(lineItems);
        if (lineItems.Count == 0)
            throw new ArgumentException("Invoice.Generate requires at least one line item.", nameof(lineItems));

        var recomputedTotal = lineItems.Aggregate(Money.Zero(subtotal.Currency), (running, item) => running + item.Amount).ClampToZero();
        if (recomputedTotal.Amount != totalDue.Amount)
            throw new ArgumentException(
                $"Invoice.Generate: supplied totalDue ({totalDue}) does not match the sum of line item amounts ({recomputedTotal}).", nameof(totalDue));

        var invoice = new Invoice
        {
            InvoiceId = InvoiceId.New(),
            TenantId = tenantId,
            Period = period,
            Subtotal = subtotal,
            TotalDue = totalDue,
            Status = InvoiceStatus.Default,
            GeneratedAtUtc = generatedAtUtc,
        };
        invoice._lineItems.AddRange(lineItems);

        invoice.AddDomainEvent(new InvoiceGeneratedEvent(Guid.NewGuid(), invoice.InvoiceId, tenantId, totalDue.Amount, totalDue.Currency, generatedAtUtc));

        return invoice;
    }

    /// <summary>Finalizes a Draft invoice and issues it to the tenant.</summary>
    public void Issue(UtcTimestamp nowUtc)
    {
        if (!Status.Equals(InvoiceStatus.Draft))
            throw new InvoiceInvalidStateTransitionException(InvoiceId, Status.Code, "Issue");

        Status = InvoiceStatus.Issued;
        IssuedAtUtc = nowUtc;

        AddDomainEvent(new InvoiceIssuedEvent(Guid.NewGuid(), InvoiceId, TenantId, nowUtc));
    }

    /// <summary>Marks an Issued (or previously PaymentFailed/Overdue) invoice as successfully paid.</summary>
    public void MarkPaid(string paymentProcessorTransactionId, UtcTimestamp nowUtc)
    {
        if (Status.Equals(InvoiceStatus.Paid) || Status.Equals(InvoiceStatus.Void))
            throw new InvoiceInvalidStateTransitionException(InvoiceId, Status.Code, "MarkPaid");
        if (string.IsNullOrWhiteSpace(paymentProcessorTransactionId))
            throw new ArgumentException("Invoice.MarkPaid requires a non-empty payment processor transaction id.", nameof(paymentProcessorTransactionId));

        Status = InvoiceStatus.Paid;
        PaidAtUtc = nowUtc;
        PaymentProcessorTransactionId = paymentProcessorTransactionId.Trim();

        AddDomainEvent(new InvoicePaidEvent(Guid.NewGuid(), InvoiceId, TenantId, PaymentProcessorTransactionId, nowUtc));
    }

    /// <summary>Records a failed charge attempt against an Issued invoice.</summary>
    public void MarkPaymentFailed(string failureReason, UtcTimestamp nowUtc)
    {
        if (Status.Equals(InvoiceStatus.Paid) || Status.Equals(InvoiceStatus.Void))
            throw new InvoiceInvalidStateTransitionException(InvoiceId, Status.Code, "MarkPaymentFailed");
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Invoice.MarkPaymentFailed requires a non-empty failure reason.", nameof(failureReason));

        Status = InvoiceStatus.PaymentFailed;

        AddDomainEvent(new InvoicePaymentFailedEvent(Guid.NewGuid(), InvoiceId, TenantId, failureReason.Trim(), nowUtc));
    }

    /// <summary>Voids an invoice before payment (e.g. billing error, tenant credit note).</summary>
    public void Void(string reason, UtcTimestamp nowUtc)
    {
        if (Status.Equals(InvoiceStatus.Paid) || Status.Equals(InvoiceStatus.Void))
            throw new InvoiceInvalidStateTransitionException(InvoiceId, Status.Code, "Void");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Invoice.Void requires a non-empty reason.", nameof(reason));

        Status = InvoiceStatus.Void;

        AddDomainEvent(new InvoiceVoidedEvent(Guid.NewGuid(), InvoiceId, TenantId, reason.Trim(), nowUtc));
    }

    /// <summary>Rehydrates an Invoice from persisted state. Infrastructure-layer use only.</summary>
    public static Invoice Rehydrate(
        InvoiceId invoiceId, TenantId tenantId, BillingPeriod period, IReadOnlyList<InvoiceLineItem> lineItems,
        Money subtotal, Money totalDue, InvoiceStatus status, UtcTimestamp generatedAtUtc,
        UtcTimestamp? issuedAtUtc, UtcTimestamp? paidAtUtc, string? paymentProcessorTransactionId)
    {
        var invoice = new Invoice
        {
            InvoiceId = invoiceId,
            TenantId = tenantId,
            Period = period,
            Subtotal = subtotal,
            TotalDue = totalDue,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            IssuedAtUtc = issuedAtUtc,
            PaidAtUtc = paidAtUtc,
            PaymentProcessorTransactionId = paymentProcessorTransactionId,
        };
        invoice._lineItems.AddRange(lineItems);
        return invoice;
    }

    private void AddDomainEvent(IBillingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
