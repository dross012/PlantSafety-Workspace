// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-30
// FILE DESCRIPTION: Query to retrieve a single WorkOrderPriority read-model record by identity,
//   including its current ETag. Closes the Phase 4 Scheduling.WorkOrderPriority gap: previously
//   there was no way for an API client to read a WorkOrderPriority record's current state before
//   issuing a mutating command. Mirrors
//   PlantSafety.Application.RiskManagement.Queries.GetRiskByIdQuery.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Models;

namespace PlantSafety.Application.Scheduling.WorkOrderPriority.Queries;

/// <summary>Retrieves a single WorkOrderPriority read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkOrderPriorityId">Identity of the WorkOrderPriority record to retrieve.</param>
public sealed record GetWorkOrderPriorityByIdQuery(
    Guid TenantId,
    Guid WorkOrderPriorityId) : IRequest<Result<WorkOrderPriorityItemDto>>;
