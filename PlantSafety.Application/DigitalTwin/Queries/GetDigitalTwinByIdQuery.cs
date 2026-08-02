// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           GetDigitalTwinByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single DigitalTwin read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.Models;

namespace PlantSafety.Application.DigitalTwin.Queries;

/// <summary>Retrieves a single DigitalTwin read-model record by identity.</summary>
public sealed record GetDigitalTwinByIdQuery(
    Guid TenantId,
    Guid DigitalTwinId) : IRequest<Result<DigitalTwinItemDto>>;
