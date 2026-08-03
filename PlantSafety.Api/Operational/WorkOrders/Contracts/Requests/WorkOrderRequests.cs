// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Api
//  File:           WorkOrderRequests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Request body contracts for every Operational.WorkOrders command endpoint.
//                  Consolidated into one file per the sibling PreventiveMaintenance module's
//                  practice of grouping small, closely-related request types. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Api.Operational.WorkOrders.Contracts.Requests;

/// <summary>Request body for <c>POST v1/maintenance/work-orders</c>.</summary>
public sealed record CreateWorkOrderRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable work order code (format WO-###).</summary>
    public required string WorkOrderCode { get; init; }

    /// <summary>Identifier of the machine/equipment targeted by this work order.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Human-readable machine code (format M-###).</summary>
    public required string MachineCode { get; init; }

    /// <summary>Priority classification (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Description of the work to be performed.</summary>
    public required string Description { get; init; }

    /// <summary>UTC instant the work order is due.</summary>
    public required DateTime DueAtUtc { get; init; }
}

/// <summary>Request body for <c>POST v1/maintenance/work-orders/{id}/assign</c>.</summary>
public sealed record AssignWorkOrderRequest
{
    /// <summary>Identifier of the technician to assign.</summary>
    public required Guid TechnicianId { get; init; }

    /// <summary>Display name of the technician to assign.</summary>
    public required string TechnicianName { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST v1/maintenance/work-orders/{id}/status</c> (Start/PutOnHold/Resume/Cancel).</summary>
public sealed record UpdateWorkOrderStatusRequest
{
    /// <summary>One of "Start", "PutOnHold", "Resume", "Cancel".</summary>
    public required string TargetStatus { get; init; }

    /// <summary>Reason text. Required for "Cancel"; optional otherwise.</summary>
    public string? Reason { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST v1/maintenance/work-orders/{id}/complete</c>.</summary>
public sealed record CompleteWorkOrderRequest
{
    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST v1/maintenance/parts/{id}/adjust-stock</c>.</summary>
public sealed record AdjustPartStockRequest
{
    /// <summary>Signed adjustment (positive = restock, negative = consumption).</summary>
    public required int DeltaQuantity { get; init; }

    /// <summary>Optional reason recorded for the adjustment.</summary>
    public string? Reason { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read.</summary>
    public string? ETag { get; init; }
}
