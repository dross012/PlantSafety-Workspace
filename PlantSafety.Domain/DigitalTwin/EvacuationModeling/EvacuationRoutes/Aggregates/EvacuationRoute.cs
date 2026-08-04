// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRoute.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single evacuation route modeled against the Digital
//                  Twin's spatial model (Route Modeling subdomain). Models the real lifecycle:
//                  definition against a validated twin model version, obstruction detection and
//                  clearance, and retirement. Carries an assembly-point reference and an
//                  in-aggregate obstruction history (RouteObstruction). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;

/// <summary>Aggregate root for a single modeled evacuation route.</summary>
public sealed partial class EvacuationRoute
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this EvacuationRoute aggregate.</summary>
    public EvacuationRouteId EvacuationRouteId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EvacuationRouteStatus Status { get; private set; }

    // ---- Route identity --------------------------------------------------------

    /// <summary>Human-readable name of this route.</summary>
    public string RouteName { get; private set; } = string.Empty;

    /// <summary>Identity of the evacuation zone this route originates from.</summary>
    public EvacuationZoneId ZoneId { get; private set; }

    /// <summary>Identity of the assembly point this route terminates at.</summary>
    public AssemblyPointId AssemblyPointId { get; private set; }

    /// <summary>Reference to the validated Digital Twin spatial-model geometry backing this route.</summary>
    public string PathGeometryRef { get; private set; } = string.Empty;

    /// <summary>Version of the Digital Twin model this route's geometry was sourced from (data-governance requirement: geometry must come only from validated model versions).</summary>
    public string TwinModelVersion { get; private set; } = string.Empty;

    /// <summary>Estimated egress time for this route under nominal conditions, in seconds.</summary>
    public int EstimatedEgressTimeSeconds { get; private set; }

    /// <summary>Cumulative count of obstruction events recorded against this route.</summary>
    public int ObstructionCount { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Obstruction history ----------------------------------------------------

    private readonly List<RouteObstructionEntry> _obstructionHistory = [];

    /// <summary>Chronological history of obstructions recorded against this route.</summary>
    public IReadOnlyList<RouteObstructionEntry> ObstructionHistory => _obstructionHistory.AsReadOnly();

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEvacuationRouteDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEvacuationRouteDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEvacuationRouteDomainEvent>(_domainEvents);

    // For rehydration only.
    private EvacuationRoute() { }

    // ---- Factory: Create (Define) -----------------------------------------------------------

    /// <summary>Defines a new evacuation route against a validated Digital Twin model version and raises <see cref="EvacuationRouteDefinedEvent"/>.</summary>
    public static EvacuationRoute Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string routeName = "",
        EvacuationZoneId zoneId = default,
        AssemblyPointId assemblyPointId = default,
        string pathGeometryRef = "",
        string twinModelVersion = "",
        int estimatedEgressTimeSeconds = 0)
    {
        if (string.IsNullOrWhiteSpace(twinModelVersion))
            throw new ArgumentException("A validated Digital Twin model version is required to define a route.", nameof(twinModelVersion));

        var agg = new EvacuationRoute
        {
            EvacuationRouteId          = EvacuationRouteId.New(),
            TenantId                   = tenantId,
            SiteId                     = siteId,
            Status                     = EvacuationRouteStatus.Active,
            RouteName                  = (routeName ?? string.Empty).Trim(),
            ZoneId                     = zoneId,
            AssemblyPointId            = assemblyPointId,
            PathGeometryRef            = (pathGeometryRef ?? string.Empty).Trim(),
            TwinModelVersion           = twinModelVersion.Trim(),
            EstimatedEgressTimeSeconds = estimatedEgressTimeSeconds,
            CreatedAtUtc               = createdAtUtc,
            CreatedBy                  = createdBy,
        };

        agg.AddDomainEvent(new EvacuationRouteDefinedEvent(
            EvacuationRouteEventId.New(), agg.EvacuationRouteId, agg.ZoneId, twinModelVersion, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordObstruction -----------------------------------------------

    /// <summary>Records a route obstruction detected on this route (e.g. via BMS feed integration) and raises <see cref="EvacuationRouteObstructedEvent"/>.</summary>
    public void RecordObstruction(
        RouteObstructionId obstructionId, string description, ObstructionSeverity severity,
        UtcTimestamp recordedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EvacuationRouteStatus.Active, EvacuationRouteStatus.Obstructed);

        Status = EvacuationRouteStatus.Obstructed;
        ObstructionCount++;

        _obstructionHistory.Add(new RouteObstructionEntry(
            obstructionId, EvacuationRouteId, description, severity, recordedAtUtc, clearedAtUtc: null));

        AddDomainEvent(new EvacuationRouteObstructedEvent(
            EvacuationRouteEventId.New(), EvacuationRouteId, obstructionId, severity, recordedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ClearObstruction -----------------------------------------------

    /// <summary>Clears the most recent open obstruction on this route and raises <see cref="EvacuationRouteClearedEvent"/>.</summary>
    public void ClearObstruction(
        RouteObstructionId obstructionId, UtcTimestamp clearedAtUtc,
        PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EvacuationRouteStatus.Obstructed);

        var index = _obstructionHistory.FindIndex(o => o.RouteObstructionId.Equals(obstructionId) && o.ClearedAtUtc is null);
        if (index < 0)
            throw new RouteObstructionNotFoundException(EvacuationRouteId, obstructionId, correlationId, causationId);

        _obstructionHistory[index] = _obstructionHistory[index].WithCleared(clearedAtUtc);

        Status = _obstructionHistory.Exists(o => o.ClearedAtUtc is null)
            ? EvacuationRouteStatus.Obstructed
            : EvacuationRouteStatus.Active;

        AddDomainEvent(new EvacuationRouteClearedEvent(
            EvacuationRouteEventId.New(), EvacuationRouteId, obstructionId, clearedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this evacuation route.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == EvacuationRouteStatus.Retired)
            throw new EvacuationRouteInvalidStatusTransitionException(EvacuationRouteId, Status.ToString(), EvacuationRouteStatus.Retired.ToString(), correlationId, causationId);

        Status = EvacuationRouteStatus.Retired;

        AddDomainEvent(new EvacuationRouteRetiredEvent(
            EvacuationRouteEventId.New(), EvacuationRouteId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEvacuationRouteDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EvacuationRouteStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EvacuationRoute {EvacuationRouteId}.");
    }
}
