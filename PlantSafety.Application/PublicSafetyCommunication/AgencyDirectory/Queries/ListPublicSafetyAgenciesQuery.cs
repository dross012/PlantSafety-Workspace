// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list PublicSafetyAgency directory records for a tenant/site,
// optionally filtered by agency type and operational status.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Queries;

/// <summary>Lists PublicSafetyAgency directory records for the given tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="AgencyType">Optional agency-type filter (e.g., "Fire", "Police"). Null returns all types.</param>
/// <param name="ActiveOnly">When <see langword="true"/>, returns only Active agencies.</param>
public sealed record ListPublicSafetyAgenciesQuery(
    Guid TenantId,
    Guid? SiteId,
    string? AgencyType,
    bool ActiveOnly) : IRequest<Result<ListPublicSafetyAgenciesQueryResult>>;
