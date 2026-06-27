// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Infrastructure
//  File:           FinancialsService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Application service facade for Financials infrastructure operations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Financials.Services;

/// <summary>Application service for Financials infrastructure operations.</summary>
public sealed class FinancialsService(
    FinancialsRepository repository,
    ILogger<FinancialsService> logger)
{
    /// <summary>Retrieves a financial record by identifier.</summary>
    public Task<FinancialsReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, tenantId, cancellationToken);
}
