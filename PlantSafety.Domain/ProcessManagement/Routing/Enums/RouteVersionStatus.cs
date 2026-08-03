// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteVersionStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status for a single immutable RouteVersionSnapshot. Genuinely new
//                  concept: ProcessDefinition's Revise increments an integer Version but never
//                  persists a queryable snapshot of prior content, so it has nothing analogous
//                  to diff against. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Routing.Enums;

/// <summary>Lifecycle status for a single immutable <c>RouteVersionSnapshot</c>.</summary>
public enum RouteVersionStatus
{
    /// <summary>Snapshot captured but not yet approved (awaiting the owning RouteDefinition's Approve).</summary>
    Pending = 0,

    /// <summary>Snapshot corresponds to an approved/in-effect revision.</summary>
    Approved = 1,

    /// <summary>Snapshot corresponds to a revision that was rejected.</summary>
    Rejected = 2,
}
