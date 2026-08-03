// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           SafetyComplianceEntry.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Derived safety-compliance rollup. Unlike HazardSummaryEntry
//                  and CorrectiveActionEntry, there is no single PlantSafety
//                  aggregate/table named "SafetyCompliance" -- no clean 1:1
//                  mapping exists. Per the task brief's explicit instruction
//                  ("derive a reasonable compliance view... from the real
//                  tables that do exist"), this is an aggregate rollup computed
//                  by dbo.usp_GetSafetyCompliance (see ..\..\sql\
//                  Phase1-StoredProcedures\03-usp-get-safety-compliance.sql) over
//                  the same two verified tables the other two entries already
//                  read from:
//                    - [RiskMgmt].[Risk]                  (one row per "Hazards" category)
//                    - [ContinuousImprovement].[CAPA]      (one row per "CorrectiveActions" category)
//
//                  "Open" / "Closed" / "Overdue" classification (see the stored
//                  procedure for the exact predicate) is derived from:
//                    - Risk.Status: RiskStatus.Closed (code "Closed", see
//                      PlantSafety.Domain.RiskManagement.Enums.RiskStatus) is the
//                      only terminal state; every other status is counted open.
//                      Risk has no due-date concept, so RiskId rows are never
//                      counted as "overdue" -- OverdueCount is always 0 for the
//                      "Hazards" category row (see stored procedure comment).
//                    - CAPA.Status: no fixed enum was found for CAPA.Status in
//                      the scanned PlantSafety code (it is a free-text
//                      NVARCHAR(50) parameter/column); the case-insensitive
//                      values 'Closed', 'Completed', and 'Cancelled' are treated
//                      as terminal, matching the vocabulary CorrectiveActionEntry
//                      already surfaces (Status). CAPA.DueDateUtc < SYSUTCDATETIME()
//                      AND not terminal => Overdue.
//
//                  This is a best-effort, evidence-grounded rollup, not a
//                  PlantSafety-owned aggregate. A human should re-review if
//                  PlantSafety ever ships a dedicated compliance/audit-score
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models;

/// <summary>A derived open/closed/overdue compliance rollup for one PlantSafety safety-data category.</summary>
/// <param name="Category">The rollup category -- "Hazards" ([RiskMgmt].[Risk]) or "CorrectiveActions" ([ContinuousImprovement].[CAPA]).</param>
/// <param name="TotalCount">Total non-deleted records in the category for the requested tenant/site/date range.</param>
/// <param name="OpenCount">Records not yet in a terminal (closed/completed/cancelled) status.</param>
/// <param name="ClosedCount">Records in a terminal status.</param>
/// <param name="OverdueCount">Open records past their due date. Always 0 for categories with no due-date concept (e.g. Hazards).</param>
/// <param name="CompliancePercentage">100 * ClosedCount / TotalCount, rounded to 2 decimal places; 100 when TotalCount is 0.</param>
/// <param name="AsOfUtc">UTC timestamp the rollup was computed (SYSUTCDATETIME() at query time).</param>
public sealed record SafetyComplianceEntry(
    string Category,
    int TotalCount,
    int OpenCount,
    int ClosedCount,
    int OverdueCount,
    decimal CompliancePercentage,
    DateTime AsOfUtc);
