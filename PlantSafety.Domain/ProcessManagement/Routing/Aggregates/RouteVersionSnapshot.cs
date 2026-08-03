// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteVersionSnapshot.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root capturing an immutable, point-in-time snapshot of a
//                  RouteDefinition's ordered step content at a specific version number. Persisted
//                  independently of the live RouteDefinition aggregate so that
//                  RouteDiffEngine.Diff can compare any two historical versions and
//                  GetRouteVersionHistoryQuery can render a timeline. Genuinely new -- Process-
//                  Definition's Revise increments an integer Version but never persists a
//                  queryable snapshot of prior structural content. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Enums;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.ProcessManagement.Routing.Aggregates;

/// <summary>Immutable, point-in-time capture of a <c>RouteDefinition</c>'s step content at a given version.</summary>
public sealed class RouteVersionSnapshot
{
    /// <summary>Unique identity of this snapshot.</summary>
    public RouteVersionId RouteVersionId { get; private set; }

    /// <summary>Identity of the owning RouteDefinition aggregate.</summary>
    public RouteDefinitionId RouteDefinitionId { get; private set; }

    /// <summary>Tenant that owns the underlying route definition.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Version number captured (matches <see cref="RouteDefinition.Version"/> at capture time).</summary>
    public int VersionNumber { get; private set; }

    /// <summary>Human-readable version label (e.g., "v3").</summary>
    public string VersionLabel => $"v{VersionNumber}";

    /// <summary>Effective date (UTC) of this version.</summary>
    public UtcTimestamp EffectiveDateUtc { get; private set; }

    /// <summary>Lifecycle status of this snapshot (Pending/Approved/Rejected).</summary>
    public RouteVersionStatus Status { get; private set; }

    /// <summary>UTC timestamp this snapshot was captured.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    private readonly List<RouteStepSnapshot> _steps = [];

    /// <summary>The captured ordered step content, exactly as it existed at capture time.</summary>
    public IReadOnlyList<RouteStepSnapshot> Steps => new ReadOnlyCollection<RouteStepSnapshot>(_steps);

    private RouteVersionSnapshot() { }

    /// <summary>Captures a new snapshot of the given route's current step content.</summary>
    public static RouteVersionSnapshot Capture(
        RouteDefinitionId routeDefinitionId, Guid tenantId, int versionNumber,
        IReadOnlyList<RouteStepSnapshot> steps, RouteVersionStatus status,
        UtcTimestamp effectiveDateUtc, UtcTimestamp createdAtUtc)
    {
        var snapshot = new RouteVersionSnapshot
        {
            RouteVersionId    = RouteVersionId.New(),
            RouteDefinitionId = routeDefinitionId,
            TenantId          = tenantId,
            VersionNumber     = versionNumber,
            Status            = status,
            EffectiveDateUtc  = effectiveDateUtc,
            CreatedAtUtc      = createdAtUtc,
        };

        snapshot._steps.AddRange(steps.OrderBy(s => s.StepNumber));
        return snapshot;
    }

    /// <summary>Rehydrates a snapshot from persisted state (Infrastructure use only).</summary>
    public static RouteVersionSnapshot Rehydrate(
        RouteVersionId routeVersionId, RouteDefinitionId routeDefinitionId, Guid tenantId, int versionNumber,
        RouteVersionStatus status, UtcTimestamp effectiveDateUtc, UtcTimestamp createdAtUtc,
        IReadOnlyList<RouteStepSnapshot> steps)
    {
        var snapshot = new RouteVersionSnapshot
        {
            RouteVersionId    = routeVersionId,
            RouteDefinitionId = routeDefinitionId,
            TenantId          = tenantId,
            VersionNumber     = versionNumber,
            Status            = status,
            EffectiveDateUtc  = effectiveDateUtc,
            CreatedAtUtc      = createdAtUtc,
        };

        snapshot._steps.AddRange(steps.OrderBy(s => s.StepNumber));
        return snapshot;
    }

    /// <summary>Marks a Pending snapshot as Approved (invoked when the owning RouteDefinition is approved).</summary>
    public void MarkApproved() => Status = RouteVersionStatus.Approved;

    /// <summary>Marks a Pending snapshot as Rejected.</summary>
    public void MarkRejected() => Status = RouteVersionStatus.Rejected;
}
