// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           GetModelRegistryEntryByIdQuery.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single ModelRegistryEntry read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Queries.GetDataLakeActivityByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.ModelRegistry.Models;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Queries;

/// <summary>Retrieves a single ModelRegistryEntry read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ModelRegistryEntryId">Identity of the model registry entry to retrieve.</param>
public sealed record GetModelRegistryEntryByIdQuery(
    Guid TenantId,
    Guid ModelRegistryEntryId) : IRequest<Result<ModelRegistryEntryDetailDto>>;
