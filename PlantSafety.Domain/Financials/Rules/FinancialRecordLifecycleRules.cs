// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialRecordLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the FinancialRecord aggregate,
//                  grounded in the pre-existing FinancialStatus enum
//                  (Draft/Submitted/Approved/Rejected/Closed/Archived). Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Rules.AuditingLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.FinancialRecord"/> aggregate.</summary>
public static class FinancialRecordLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a line item may be added while in <paramref name="current"/>.</summary>
    public static bool CanAddLineItem(FinancialStatus current) => current == FinancialStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the record may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmit(FinancialStatus current) => current == FinancialStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the record may be approved from <paramref name="current"/>.</summary>
    public static bool CanApprove(FinancialStatus current) => current == FinancialStatus.Submitted;

    /// <summary>Returns <see langword="true"/> when the record may be rejected from <paramref name="current"/>.</summary>
    public static bool CanReject(FinancialStatus current) => current == FinancialStatus.Submitted;

    /// <summary>Returns <see langword="true"/> when the record may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(FinancialStatus current) => current == FinancialStatus.Approved;

    /// <summary>Returns <see langword="true"/> when the record may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(FinancialStatus current) => current == FinancialStatus.Closed || current == FinancialStatus.Rejected;
}
