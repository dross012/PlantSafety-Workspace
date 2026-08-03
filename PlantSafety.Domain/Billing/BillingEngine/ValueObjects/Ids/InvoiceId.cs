// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           InvoiceId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for an Invoice aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;

/// <summary>Strong-typed identity for an <see cref="Aggregates.Invoice"/>.</summary>
public readonly record struct InvoiceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an InvoiceId wrapping a non-empty GUID.</summary>
    public InvoiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("InvoiceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new InvoiceId using Uuid7.</summary>
    public static InvoiceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an InvoiceId.</summary>
    public static InvoiceId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
