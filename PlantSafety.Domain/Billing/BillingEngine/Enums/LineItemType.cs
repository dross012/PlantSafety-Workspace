// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           LineItemType.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Classifies a single InvoiceLineItem. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Billing.BillingEngine.Enums;

/// <summary>Classifies a single <see cref="ValueObjects.InvoiceLineItem"/>.</summary>
public enum LineItemType
{
    /// <summary>The plan's recurring monthly base price.</summary>
    BasePlan = 0,

    /// <summary>A usage-based overage charge for a single metered dimension.</summary>
    Overage = 1,

    /// <summary>A negative-amount discount reducing the invoice subtotal.</summary>
    Discount = 2,

    /// <summary>A negative-amount account credit reducing the invoice subtotal.</summary>
    Credit = 3,
}
