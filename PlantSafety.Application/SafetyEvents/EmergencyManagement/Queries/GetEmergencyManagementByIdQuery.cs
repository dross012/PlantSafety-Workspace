// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           GetEmergencyManagementByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single EmergencyManagement read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Queries;

/// <summary>Retrieves a single EmergencyManagement read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EmergencyManagementId">Identity of the EmergencyManagement record to retrieve.</param>
public sealed record GetEmergencyManagementByIdQuery(
    Guid TenantId,
    Guid EmergencyManagementId) : IRequest<Result<EmergencyManagementItemDto>>;
