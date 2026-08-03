// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteStepStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic readiness classification for a single RouteStep, computed by
//                  RouteStep.ComputeStatus() -- not a client-settable field. Genuinely new
//                  concept: ProcessDefinition has no notion of ordered steps at all.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Routing.Enums;

/// <summary>Deterministic readiness classification for a single <c>RouteStep</c>.</summary>
public enum RouteStepStatus
{
    /// <summary>Step has instructions and at least one assigned resource.</summary>
    Valid = 0,

    /// <summary>Step has instructions but no assigned resources.</summary>
    Warning = 1,

    /// <summary>Step has no instructions (cannot be executed as documented).</summary>
    Blocked = 2,
}
