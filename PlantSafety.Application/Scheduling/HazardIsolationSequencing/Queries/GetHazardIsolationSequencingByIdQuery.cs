// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           GetHazardIsolationSequencingByIdQuery.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single HazardIsolationSequencing read-model record by
//                  identity, including its current ETag, so a caller can read-before-write into one
//                  of the lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Models;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Queries;

/// <summary>Retrieves a single HazardIsolationSequencing read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="HazardIsolationSequencingId">Identity of the HazardIsolationSequencing record to retrieve.</param>
public sealed record GetHazardIsolationSequencingByIdQuery(
    Guid TenantId,
    Guid HazardIsolationSequencingId) : IRequest<Result<HazardIsolationSequencingItemDto>>;
