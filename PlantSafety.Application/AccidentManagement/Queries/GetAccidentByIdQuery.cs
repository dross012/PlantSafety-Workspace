// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Query to retrieve a single Accident read-model record by identity, including
//   its current ETag. Closes the Phase 4 AccidentManagement gap: previously there was no way for
//   an API client to read an Accident's current state before issuing a mutating command. Mirrors
//   PlantSafety.Application.RiskManagement.Queries.GetRiskByIdQuery.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.AccidentManagement.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AccidentManagement.Queries;

/// <summary>Retrieves a single Accident read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AccidentId">Identity of the Accident to retrieve.</param>
public sealed record GetAccidentByIdQuery(
    Guid TenantId,
    Guid AccidentId) : IRequest<Result<AccidentItemDto>>;
