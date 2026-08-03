// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyRecommendationPriority.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Simple enum (Handbook §2.7.B) for the urgency of a structured safety
//                  recommendation. No per-value metadata/behavior beyond ordering, so a Simple Enum
//                  is the correct choice per the 2026 Enums Architectural Rule.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Enums;

/// <summary>Urgency classification for a <see cref="ValueObjects.SafetyRecommendation"/>.</summary>
public enum GlobalSafetyRecommendationPriority
{
    /// <summary>Informational; no immediate action required.</summary>
    Low = 0,

    /// <summary>Should be addressed in the normal planning cycle.</summary>
    Medium = 1,

    /// <summary>Should be addressed promptly.</summary>
    High = 2,

    /// <summary>Requires immediate cross-site coordination.</summary>
    Critical = 3,
}
