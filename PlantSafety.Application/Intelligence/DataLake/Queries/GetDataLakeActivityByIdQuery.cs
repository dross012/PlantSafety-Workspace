// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           GetDataLakeActivityByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single DataLakeActivity read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Distinct from the pre-existing, untouched date-range
//                  GetDataLakeQuery. Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Queries.GetRCAByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.DataLake.Models;

namespace PlantSafety.Application.Intelligence.DataLake.Queries;

/// <summary>Retrieves a single DataLakeActivity read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="DataLakeActivityId">Identity of the data lake activity record to retrieve.</param>
public sealed record GetDataLakeActivityByIdQuery(
    Guid TenantId,
    Guid DataLakeActivityId) : IRequest<Result<DataLakeActivityDetailDto>>;
