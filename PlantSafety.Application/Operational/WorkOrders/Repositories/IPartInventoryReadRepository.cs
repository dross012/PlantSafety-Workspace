// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           IPartInventoryReadRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Part inventory list. Implemented in
//                  the Infrastructure layer (Dapper). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.WorkOrders.Models;

namespace PlantSafety.Application.Operational.WorkOrders.Repositories;

/// <summary>Read-only repository for the Part inventory list. Implemented in the Infrastructure layer.</summary>
public interface IPartInventoryReadRepository
{
    /// <summary>Returns a paginated page of Part inventory rows for the given tenant.</summary>
    Task<(IReadOnlyList<PartInventoryDto> Items, int TotalCount)> ListAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
