// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for per-module NFPA coverage exposure across an entire tenant (any
//   site) -- backs the "tenant-wide exposure" tile. Also records a tenant-wide snapshot per
//   module.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Returns per-module NFPA coverage exposure across the whole tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
public sealed record GetNfpaTenantExposureQuery(
    Guid TenantId) : IRequest<Result<IReadOnlyList<NfpaGapSummaryDto>>>;
