// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the LotoProcedures aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="LotoProcedures"/> aggregate.</summary>
public static class LotoProceduresLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the procedure may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(LotoProceduresStatus current) =>
        current is LotoProceduresStatus.Draft or LotoProceduresStatus.UnderRevision;

    /// <summary>Returns <see langword="true"/> when the procedure may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(LotoProceduresStatus current) => current == LotoProceduresStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when the annual audit may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordAnnualAudit(LotoProceduresStatus current) =>
        current is LotoProceduresStatus.Published or LotoProceduresStatus.AuditFlagged;

    /// <summary>Returns <see langword="true"/> when a revision may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleRevision(LotoProceduresStatus current) =>
        current is LotoProceduresStatus.Published or LotoProceduresStatus.AuditFlagged;
}
