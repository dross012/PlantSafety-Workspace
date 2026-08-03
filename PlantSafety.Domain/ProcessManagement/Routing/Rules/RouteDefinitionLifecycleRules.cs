// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionLifecycleRules.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the RouteDefinition aggregate.
//                  Identical shape to
//                  PlantSafety.Domain.ProcessManagement.Rules.ProcessDefinitionLifecycleRules --
//                  Routing reuses ProcessManagement's approval/versioning lifecycle wholesale.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Enums;

namespace PlantSafety.Domain.ProcessManagement.Routing.Rules;

/// <summary>Defines permitted lifecycle transitions for the <c>RouteDefinition</c> aggregate.</summary>
public static class RouteDefinitionLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the route may be submitted for review from <paramref name="current"/>.</summary>
    public static bool CanSubmitForReview(RouteDefinitionStatus current) => current == RouteDefinitionStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the route may be approved from <paramref name="current"/>.</summary>
    public static bool CanApprove(RouteDefinitionStatus current) => current == RouteDefinitionStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the route may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(RouteDefinitionStatus current) => current == RouteDefinitionStatus.Active;

    /// <summary>Returns <see langword="true"/> when the route may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(RouteDefinitionStatus current) => current == RouteDefinitionStatus.Retired;

    /// <summary>Returns <see langword="true"/> when the route's steps may be revised (same-status action) from <paramref name="current"/>.</summary>
    public static bool CanRevise(RouteDefinitionStatus current) => current == RouteDefinitionStatus.Active;
}
