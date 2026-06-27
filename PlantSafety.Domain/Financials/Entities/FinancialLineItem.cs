// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialLineItem.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Entity representing a line item in a financial record.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Entities;

/// <summary>Represents a single line item within a financial record.</summary>
public sealed class FinancialLineItem
{
    /// <summary>Unique identity of this line item.</summary>
    public Guid LineItemId { get; private set; }
    /// <summary>Description of the line item.</summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>Amount in the site's base currency.</summary>
    public decimal Amount { get; private set; }
    /// <summary>For ORM rehydration only.</summary>
    private FinancialLineItem() { }
    /// <summary>Creates a new financial line item.</summary>
    public FinancialLineItem(Guid lineItemId, string description, decimal amount)
    {
        LineItemId  = lineItemId;
        Description = description;
        Amount      = amount;
    }
}
