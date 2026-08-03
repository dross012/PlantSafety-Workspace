// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           InvoiceDomainException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Base exception type for Invoice aggregate domain rule violations.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Billing.BillingEngine.Exceptions;

/// <summary>Base exception for Invoice aggregate domain rule violations.</summary>
public class InvoiceDomainException : Exception
{
    /// <summary>The invoice this exception concerns.</summary>
    public InvoiceId InvoiceId { get; }

    /// <summary>Creates a new InvoiceDomainException.</summary>
    public InvoiceDomainException(InvoiceId invoiceId, string message) : base(message) => InvoiceId = invoiceId;
}

/// <summary>Thrown when an Invoice lifecycle transition is attempted from a status that does not permit it.</summary>
public sealed class InvoiceInvalidStateTransitionException : InvoiceDomainException
{
    /// <summary>Creates a new InvoiceInvalidStateTransitionException.</summary>
    public InvoiceInvalidStateTransitionException(InvoiceId invoiceId, string currentStatus, string attemptedTransition)
        : base(invoiceId, $"Invoice {invoiceId} cannot transition via '{attemptedTransition}' from status '{currentStatus}'.")
    {
    }
}
