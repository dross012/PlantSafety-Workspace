// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ListMutualAidAgreementsQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query backing the Mutual Aid Agreement Registry dashboard. Mirrors
//                  GetEmergencyManagementQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;

/// <summary>Lists mutual aid agreements for the given tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns agreements for all accessible sites.</param>
public sealed record ListMutualAidAgreementsQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<ListMutualAidAgreementsResult>>;

/// <summary>Result of <see cref="ListMutualAidAgreementsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching mutual aid agreements.</param>
public sealed record ListMutualAidAgreementsResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MutualAidAgreementItemDto> Items);
