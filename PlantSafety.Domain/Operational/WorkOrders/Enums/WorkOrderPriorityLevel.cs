// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderPriorityLevel.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Priority classification for the WorkOrder aggregate root. Mirrors the
//                  Low/Medium/High/Critical vocabulary already expected by the Maintenance UI's
//                  WorkOrderRowDto.Priority field. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.Enums;

/// <summary>Priority classification for a WorkOrder.</summary>
public enum WorkOrderPriorityLevel
{
    /// <summary>Routine work with no immediate safety/production impact.</summary>
    Low = 0,

    /// <summary>Standard scheduling priority.</summary>
    Medium = 1,

    /// <summary>Elevated priority -- expedite scheduling.</summary>
    High = 2,

    /// <summary>Immediate attention required (safety or production-stopping impact).</summary>
    Critical = 3,
}
