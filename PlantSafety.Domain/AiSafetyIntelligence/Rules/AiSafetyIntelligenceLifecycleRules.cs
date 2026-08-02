// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           AiSafetyIntelligenceLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the AiSafetyIntelligence aggregate.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Rules;

/// <summary>Defines permitted lifecycle transitions for the AiSafetyIntelligence aggregate.</summary>
public static class AiSafetyIntelligenceLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(AiSafetyIntelligenceStatus current) => current == AiSafetyIntelligenceStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(AiSafetyIntelligenceStatus current) => current == AiSafetyIntelligenceStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AiSafetyIntelligenceStatus current) => current == AiSafetyIntelligenceStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AiSafetyIntelligenceStatus current) => current == AiSafetyIntelligenceStatus.Closed;
}
