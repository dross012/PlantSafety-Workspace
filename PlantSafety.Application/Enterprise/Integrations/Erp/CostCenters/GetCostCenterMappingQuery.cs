// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Application
//  File:           GetCostCenterMappingQuery.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve the effective cost center mapping for a Tenant/Site. Additive-
//                  only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;

/// <summary>Retrieves the effective cost center mapping for a Tenant/Site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site identifier, or null to request the tenant-wide default mapping.</param>
public sealed record GetCostCenterMappingQuery(Guid TenantId, Guid? SiteId) : IRequest<Result<CostCenterMapping>>;

/// <summary>Handles <see cref="GetCostCenterMappingQuery"/>.</summary>
public sealed class GetCostCenterMappingQueryHandler(ICostCenterMappingRepository repository)
    : IRequestHandler<GetCostCenterMappingQuery, Result<CostCenterMapping>>
{
    /// <inheritdoc/>
    public async Task<Result<CostCenterMapping>> Handle(GetCostCenterMappingQuery request, CancellationToken cancellationToken)
    {
        var mapping = await repository.GetAsync(request.TenantId, request.SiteId, cancellationToken);

        return mapping is null
            ? Result<CostCenterMapping>.Failure(new ApplicationError(
                "Enterprise.Integrations.Erp.CostCenterMapping.NotFound",
                "No cost center mapping is configured for this Tenant/Site.",
                Guid.NewGuid()))
            : Result<CostCenterMapping>.Success(mapping);
    }
}
