// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Search
//  Layer:          Infrastructure
//  File:           SearchIndexBase.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for Azure AI Search index definition records.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Search.IndexDefinitions;

/// <summary>Abstract base for Azure AI Search index definition classes.</summary>
public abstract record SearchIndexBase
{
    /// <summary>Unique document key in the search index.</summary>
    public abstract string Key { get; }

    /// <summary>Module that owns this indexed document.</summary>
    public abstract string Module { get; }

    /// <summary>Tenant scoping field — all search queries must be tenant-filtered.</summary>
    public string TenantId { get; init; } = string.Empty;
}
