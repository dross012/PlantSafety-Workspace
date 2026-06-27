// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainSeverity.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Cross-cutting severity classification used across all modules for
//      exceptions, alerts, incidents, and observations.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Types.Enums;

/// <summary>Canonical severity levels used across PlantSafety modules.</summary>
public enum DomainSeverity
{
    /// <summary>Informational — no action required.</summary>
    Info = 0,

    /// <summary>Low severity — monitor and log.</summary>
    Low = 1,

    /// <summary>Medium severity — requires attention within the shift.</summary>
    Medium = 2,

    /// <summary>High severity — requires immediate attention.</summary>
    High = 3,

    /// <summary>Critical severity — safety-of-life or production-stopping.</summary>
    Critical = 4,
}
