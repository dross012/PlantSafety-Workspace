// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the PermitToWork aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PermitToWork"/> aggregate.</summary>
public static class PermitToWorkLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a hazard assessment may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordHazardAssessment(PermitToWorkStatus current) => current == PermitToWorkStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the permit may be issued from <paramref name="current"/>.</summary>
    public static bool CanIssue(PermitToWorkStatus current) => current == PermitToWorkStatus.HazardAssessed;

    /// <summary>Returns <see langword="true"/> when the permit may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(PermitToWorkStatus current) => current == PermitToWorkStatus.Issued;

    /// <summary>Returns <see langword="true"/> when the permit may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PermitToWorkStatus current) => current is PermitToWorkStatus.Issued or PermitToWorkStatus.Suspended;

    /// <summary>Returns <see langword="true"/> when the permit may expire from <paramref name="current"/>.</summary>
    public static bool CanExpire(PermitToWorkStatus current) => current == PermitToWorkStatus.Issued;
}
