// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Events
//  File:           LogCategory.cs
//  Created:        2026-08-09
//  Description:    Phase 2.4 deliverable: the module-level log categories.
//                  Matches PlantSafety's real bounded contexts (not the
//                  generic Routing/Scheduling/Maintenance/ProductionTracking
//                  names from the original, unadapted spec).
// ============================================================================
namespace Shared.Telemetry.Abstractions.Events;

/// <summary>The module a log event originated from.</summary>
public enum LogCategory
{
    /// <summary>Shared.FacilityDomain / the Facility module.</summary>
    Facility,

    /// <summary>Shared.EmployeeDomain / the Employee module.</summary>
    Employee,

    /// <summary>Shared.MachineDomain / the Machine module.</summary>
    Machine,

    /// <summary>Shared.WorkflowEngine / Enterprise.WorkflowOrchestration.</summary>
    WorkflowEngine,

    /// <summary>Shared.AuditLogging.</summary>
    Audit,

    /// <summary>Shared.Authorization.</summary>
    Authorization,

    /// <summary>Shared.Authorization's API-key authentication layer (Phase 10).</summary>
    Authentication,

    /// <summary>Shared.Authorization's multi-tenant/contextual access layer (Phase 11).</summary>
    Tenant,

    /// <summary>Any AI.* engine (Path E).</summary>
    AiEngine,

    /// <summary>Any Enterprise.* engine (Path D).</summary>
    EnterpriseEngine,

    /// <summary>Cross-cutting API/host infrastructure not specific to one module above.</summary>
    Infrastructure,
}
