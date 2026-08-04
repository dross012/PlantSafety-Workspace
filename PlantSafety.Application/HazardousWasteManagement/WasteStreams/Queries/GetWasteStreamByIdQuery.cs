// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single WasteStream by identifier.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.WasteStreams.Queries;

/// <summary>Retrieves a single WasteStream read-model item by identifier.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WasteStreamId">Identity of the WasteStream to retrieve.</param>
public sealed record GetWasteStreamByIdQuery(
    Guid TenantId,
    Guid WasteStreamId) : IRequest<Result<GetWasteStreamByIdResult>>;
