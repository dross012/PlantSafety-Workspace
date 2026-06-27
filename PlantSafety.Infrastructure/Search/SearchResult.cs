// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Search
//  Layer:          Infrastructure
//  File:           SearchResult.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Wrapper record for a single search result with relevance score.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Search;

/// <summary>Wrapper for a single search result with relevance score.</summary>
public sealed record SearchResult<T>(T Document, double Score);
