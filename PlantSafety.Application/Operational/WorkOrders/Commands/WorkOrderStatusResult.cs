// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           WorkOrderStatusResult.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for every WorkOrder command that only needs to echo back
//                  the resulting identity/status. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>Result of a WorkOrder lifecycle command.</summary>
/// <param name="WorkOrderId">Identity of the affected WorkOrder aggregate.</param>
/// <param name="Status">Lifecycle status after the command executed.</param>
public sealed record WorkOrderStatusResult(Guid WorkOrderId, string Status);
