// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Infrastructure
//  File:           FinancialsReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model for Financials queries.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Financials.Persistence;

/// <summary>Dapper read model returned by Financials stored procedures.</summary>
public sealed record FinancialsReadModel
{
    public Guid FinancialId { get; init; }
    public string Category { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public Guid TenantId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}
