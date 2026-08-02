// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinitionLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the ProcessDefinition aggregate.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="ProcessDefinition"/> aggregate.</summary>
public static class ProcessDefinitionLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the process may be submitted for review from <paramref name="current"/>.</summary>
    public static bool CanSubmitForReview(ProcessDefinitionStatus current) => current == ProcessDefinitionStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the process may be approved from <paramref name="current"/>.</summary>
    public static bool CanApprove(ProcessDefinitionStatus current) => current == ProcessDefinitionStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the process may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(ProcessDefinitionStatus current) => current == ProcessDefinitionStatus.Active;

    /// <summary>Returns <see langword="true"/> when the process may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(ProcessDefinitionStatus current) => current == ProcessDefinitionStatus.Retired;

    /// <summary>Returns <see langword="true"/> when the process may be revised (same-status action) from <paramref name="current"/>.</summary>
    public static bool CanRevise(ProcessDefinitionStatus current) => current == ProcessDefinitionStatus.Active;
}
