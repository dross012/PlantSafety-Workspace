// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           ContractorIncidentCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of contractor incident categories per the PlantSafety Program
//                  Structure Incident Tracking & Performance Evaluation component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Enums;

/// <summary>Classification of contractor incident categories.</summary>
public enum ContractorIncidentCategory
{
    /// <summary>Recordable incident.</summary>
    Incident = 0,

    /// <summary>Near-miss event.</summary>
    NearMiss = 1,

    /// <summary>Unsafe condition observed but not resulting in injury.</summary>
    UnsafeCondition = 2,
}
