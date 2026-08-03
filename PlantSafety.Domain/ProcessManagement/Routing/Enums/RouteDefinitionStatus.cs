// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status vocabulary for the RouteDefinition aggregate root. Mirrors
//                  PlantSafety.Domain.ProcessManagement.Enums.ProcessDefinitionStatus's five-state
//                  Draft/UnderReview/Active/Retired/Archived vocabulary -- Routing reuses
//                  ProcessManagement's approval/versioning lifecycle wholesale rather than
//                  inventing a new one (see docs/Phase5/ui-gaps-scoping.md §9).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Routing.Enums;

/// <summary>Lifecycle states for the <c>RouteDefinition</c> aggregate root.</summary>
public enum RouteDefinitionStatus
{
    /// <summary>Route definition is being authored and has not yet entered review.</summary>
    Draft = 0,

    /// <summary>Route definition has been submitted and is awaiting approval.</summary>
    UnderReview = 1,

    /// <summary>Route definition has been approved and is in effect.</summary>
    Active = 2,

    /// <summary>Route definition has been withdrawn from active use.</summary>
    Retired = 3,

    /// <summary>Route definition has been archived for long-term retention.</summary>
    Archived = 4,
}
