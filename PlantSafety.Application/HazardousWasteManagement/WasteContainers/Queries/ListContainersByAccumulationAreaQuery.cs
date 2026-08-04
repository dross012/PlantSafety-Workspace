// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list WasteContainer records scoped to a single AccumulationArea.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.WasteContainers.Queries;

/// <summary>Lists all WasteContainer items within the given accumulation area.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AccumulationAreaId">The accumulation area to list containers for.</param>
public sealed record ListContainersByAccumulationAreaQuery(
    Guid TenantId,
    Guid AccumulationAreaId) : IRequest<Result<ListContainersByAccumulationAreaResult>>;
