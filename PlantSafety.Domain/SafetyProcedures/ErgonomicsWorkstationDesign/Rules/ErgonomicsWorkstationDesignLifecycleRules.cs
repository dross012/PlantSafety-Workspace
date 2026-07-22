// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the ErgonomicsWorkstationDesign
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="ErgonomicsWorkstationDesign"/> aggregate.</summary>
public static class ErgonomicsWorkstationDesignLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the procedure may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(ErgonomicsWorkstationDesignStatus current) => current == ErgonomicsWorkstationDesignStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the procedure may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(ErgonomicsWorkstationDesignStatus current) => current == ErgonomicsWorkstationDesignStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when an assessment may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleAssessment(ErgonomicsWorkstationDesignStatus current) => current == ErgonomicsWorkstationDesignStatus.Published;

    /// <summary>Returns <see langword="true"/> when an assessment may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordAssessment(ErgonomicsWorkstationDesignStatus current) => current == ErgonomicsWorkstationDesignStatus.AssessmentScheduled;
}
