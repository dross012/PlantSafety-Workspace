// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           SustainabilityTargetStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Progress status for a SustainabilityTarget child entity, evaluated by
//                  SustainabilityTargetRiskPolicy on every progress update.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;

/// <summary>Progress status for a SustainabilityTarget child entity.</summary>
public enum SustainabilityTargetStatus
{
    /// <summary>Target has been set and is tracking toward its target date on or ahead of plan.</summary>
    OnTrack = 0,

    /// <summary>Target is trending behind plan; feeds RiskManagement as an at-risk condition.</summary>
    AtRisk = 1,

    /// <summary>Target has been met on or before its target date.</summary>
    Achieved = 2,

    /// <summary>Target date passed without the target being met.</summary>
    Missed = 3,
}
