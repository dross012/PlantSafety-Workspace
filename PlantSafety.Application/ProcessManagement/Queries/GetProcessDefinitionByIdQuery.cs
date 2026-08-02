// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           GetProcessDefinitionByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single ProcessDefinition read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Queries.GetAuditingByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Models;

namespace PlantSafety.Application.ProcessManagement.Queries;

/// <summary>Retrieves a single ProcessDefinition read-model record by identity.</summary>
public sealed record GetProcessDefinitionByIdQuery(
    Guid TenantId,
    Guid ProcessDefinitionId) : IRequest<Result<ProcessDefinitionItemDto>>;
