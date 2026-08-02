// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Query to retrieve a single Risk read-model record by identity, including its
//   current ETag. Closes the Phase 4 RiskManagement gap: previously there was no way for an API
//   client to read a Risk's current state before issuing a mutating command.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.Models;

namespace PlantSafety.Application.RiskManagement.Queries;

/// <summary>Retrieves a single Risk read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="RiskId">Identity of the Risk to retrieve.</param>
public sealed record GetRiskByIdQuery(
    Guid TenantId,
    Guid RiskId) : IRequest<Result<RiskItemDto>>;
