// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           AssemblyPoint.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Read-oriented entity describing a designated evacuation assembly point that
//                  EvacuationRoute aggregates terminate at. Modeled as an entity (not its own
//                  aggregate) because assembly points have no independent lifecycle beyond simple
//                  registration and are always accessed in the context of a route. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Entities;

/// <summary>A designated evacuation assembly point referenced by one or more evacuation routes.</summary>
public sealed class AssemblyPoint
{
    /// <summary>Identity of this assembly point.</summary>
    public AssemblyPointId AssemblyPointId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Human-readable name of the assembly point.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Rated headcount capacity of the assembly point.</summary>
    public int Capacity { get; private set; }

    /// <summary>Reference to the Digital Twin spatial-model coordinates of this assembly point.</summary>
    public string GeoReference { get; private set; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private AssemblyPoint() { }

    /// <summary>Creates a new assembly point registration.</summary>
    public AssemblyPoint(
        AssemblyPointId assemblyPointId, Guid tenantId, Guid? siteId,
        string name, int capacity, string geoReference, UtcTimestamp createdAtUtc)
    {
        AssemblyPointId = assemblyPointId;
        TenantId        = tenantId;
        SiteId          = siteId;
        Name            = name;
        Capacity        = capacity;
        GeoReference    = geoReference;
        CreatedAtUtc    = createdAtUtc;
    }
}
