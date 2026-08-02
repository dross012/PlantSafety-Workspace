// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernanceLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the AiGovernance aggregate. Phase 4
//                  full build-out. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Rules.AuditingLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Rules;

/// <summary>Defines permitted lifecycle transitions for the AiGovernance aggregate.</summary>
public static class AiGovernanceLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(AiGovernanceStatus current) => current == AiGovernanceStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(AiGovernanceStatus current) => current == AiGovernanceStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AiGovernanceStatus current) => current == AiGovernanceStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AiGovernanceStatus current) => current == AiGovernanceStatus.Closed;
}
