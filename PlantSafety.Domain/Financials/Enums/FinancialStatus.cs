// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialStatus.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status for financial records.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Enums;

/// <summary>Lifecycle states for financial records.</summary>
public enum FinancialStatus
{
    /// <summary>Record is a draft pending approval.</summary>
    Draft = 0,
    /// <summary>Record has been submitted for approval.</summary>
    Submitted = 1,
    /// <summary>Record has been approved and is active.</summary>
    Approved = 2,
    /// <summary>Record has been rejected.</summary>
    Rejected = 3,
    /// <summary>Record has been closed.</summary>
    Closed = 4,
    /// <summary>Record has been archived.</summary>
    Archived = 5,
}
