// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side request/response DTOs for the Operational.
//   WorkOrders write surface (CreateWorkOrderCommand, AssignWorkOrderCommand,
//   UpdateWorkOrderStatusCommand, CompleteWorkOrderCommand,
//   AdjustPartStockCommand). Field-for-field mirrors of
//   PlantSafety.Api.Operational.WorkOrders.Contracts.Requests.WorkOrderRequests.cs
//   and the corresponding Application-layer command result records. No
//   ETag threading — the read-side DTOs (WorkOrderDetailDto/PartInventoryDto)
//   don't carry one, so every write here omits it (server treats null as
//   "skip concurrency check").
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Maintenance.Models;

/// <summary>
/// Request body for creating a new work order. Mirrors
/// PlantSafety.Api.Operational.WorkOrders.Contracts.Requests.CreateWorkOrderRequest,
/// with DataAnnotations mirroring CreateWorkOrderCommandValidator's rules.
/// Backed by POST v1/maintenance/work-orders.
/// </summary>
public sealed class CreateWorkOrderRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Human-readable work order code (format WO-###).</summary>
    [Required(ErrorMessage = "Work order code is required.")]
    [StringLength(50, ErrorMessage = "Work order code must be 50 characters or fewer.")]
    public string WorkOrderCode { get; set; } = string.Empty;

    /// <summary>Identifier of the machine/equipment targeted by this work order.</summary>
    [Required(ErrorMessage = "Machine is required.")]
    public Guid? MachineId { get; set; }

    /// <summary>Human-readable machine code (format M-###).</summary>
    [Required(ErrorMessage = "Machine code is required.")]
    [StringLength(50, ErrorMessage = "Machine code must be 50 characters or fewer.")]
    public string MachineCode { get; set; } = string.Empty;

    /// <summary>Priority classification (Low, Medium, High, Critical).</summary>
    [Required(ErrorMessage = "Priority is required.")]
    public string Priority { get; set; } = "Medium";

    /// <summary>Description of the work to be performed.</summary>
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description must be 2000 characters or fewer.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>UTC instant the work order is due.</summary>
    [Required(ErrorMessage = "Due date is required.")]
    public DateTime? DueAtUtc { get; set; }
}

/// <summary>Result of a successful <see cref="CreateWorkOrderRequest"/> submission.</summary>
public sealed class CreateWorkOrderResult
{
    /// <summary>Gets the identifier of the newly created work order.</summary>
    public Guid WorkOrderId { get; init; }

    /// <summary>Gets the human-readable work order code.</summary>
    public string WorkOrderCode { get; init; } = string.Empty;

    /// <summary>Gets the lifecycle status of the newly created record.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for (re)assigning a technician to an existing work order.</summary>
public sealed class AssignWorkOrderRequest
{
    /// <summary>Identifier of the technician to assign.</summary>
    public Guid TechnicianId { get; set; }

    /// <summary>Display name of the technician to assign.</summary>
    public string TechnicianName { get; set; } = string.Empty;

    /// <summary>Optimistic-concurrency token from a prior read. Never populated client-side.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for applying a Start/PutOnHold/Resume/Cancel lifecycle transition.</summary>
public sealed class UpdateWorkOrderStatusRequest
{
    /// <summary>One of "Start", "PutOnHold", "Resume", "Cancel".</summary>
    public string TargetStatus { get; set; } = string.Empty;

    /// <summary>Reason text. Required for "Cancel"; optional otherwise.</summary>
    public string? Reason { get; set; }

    /// <summary>Optimistic-concurrency token from a prior read. Never populated client-side.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for completing an in-progress work order.</summary>
public sealed class CompleteWorkOrderRequest
{
    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; set; }

    /// <summary>Optimistic-concurrency token from a prior read. Never populated client-side.</summary>
    public string? ETag { get; set; }
}

/// <summary>Result of a successful assign/status/complete lifecycle command.</summary>
public sealed class WorkOrderStatusResult
{
    /// <summary>Gets the affected work order identifier.</summary>
    public Guid WorkOrderId { get; init; }

    /// <summary>Gets the resulting lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for adjusting a part's on-hand stock quantity.</summary>
public sealed class AdjustPartStockRequest
{
    /// <summary>Signed adjustment (positive = restock, negative = consumption). Must be non-zero.</summary>
    public int DeltaQuantity { get; set; }

    /// <summary>Optional reason recorded for the adjustment.</summary>
    public string? Reason { get; set; }

    /// <summary>Optimistic-concurrency token from a prior read. Never populated client-side.</summary>
    public string? ETag { get; set; }
}

/// <summary>Result of a successful part stock adjustment.</summary>
public sealed class PartStockResult
{
    /// <summary>Gets the affected part identifier.</summary>
    public Guid PartId { get; init; }

    /// <summary>Gets the resulting on-hand quantity.</summary>
    public int QuantityOnHand { get; init; }
}
