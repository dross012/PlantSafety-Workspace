// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Application
//  File:           SetCostCenterMappingCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Command to create or replace a Tenant/Site -&gt; external ERP cost center code
//                  mapping. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;

/// <summary>Creates or replaces a Tenant/Site cost center mapping.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site identifier, or null for a tenant-wide default mapping.</param>
/// <param name="ExternalCostCenterCode">The external ERP system's cost center code.</param>
/// <param name="Description">Human-readable description of the cost center.</param>
/// <param name="UpdatedByUserId">Identity of the user setting this mapping.</param>
public sealed record SetCostCenterMappingCommand(
    Guid TenantId,
    Guid? SiteId,
    string ExternalCostCenterCode,
    string? Description,
    Guid UpdatedByUserId) : IRequest<Result<CostCenterMapping>>;

/// <summary>Handles <see cref="SetCostCenterMappingCommand"/>.</summary>
public sealed class SetCostCenterMappingCommandHandler(ICostCenterMappingRepository repository)
    : IRequestHandler<SetCostCenterMappingCommand, Result<CostCenterMapping>>
{
    /// <inheritdoc/>
    public async Task<Result<CostCenterMapping>> Handle(SetCostCenterMappingCommand request, CancellationToken cancellationToken)
    {
        var code = (request.ExternalCostCenterCode ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<CostCenterMapping>.Failure(new ApplicationError(
                "Enterprise.Integrations.Erp.CostCenterMapping.CodeRequired",
                "ExternalCostCenterCode must be supplied.",
                Guid.NewGuid()));
        }

        var mapping = new CostCenterMapping(
            TenantId: request.TenantId,
            SiteId: request.SiteId,
            ExternalCostCenterCode: code,
            Description: string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            UpdatedAtUtc: UtcTimestamp.From(DateTime.UtcNow),
            UpdatedBy: new UserId(request.UpdatedByUserId));

        await repository.UpsertAsync(mapping, cancellationToken);

        return Result<CostCenterMapping>.Success(mapping);
    }
}
