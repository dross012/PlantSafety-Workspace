// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           GetWorkerWellBeingByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single WorkerWellBeing read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.WorkerWellBeing.Models;

namespace PlantSafety.Application.WorkerWellBeing.Queries;

/// <summary>Retrieves a single WorkerWellBeing read-model record by identity.</summary>
public sealed record GetWorkerWellBeingByIdQuery(
    Guid TenantId,
    Guid WorkerWellBeingId) : IRequest<Result<WorkerWellBeingItemDto>>;
