// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GlobalSafetyKpiSectionRequest.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 9 §5.6 Compliance Report Generation)
//  SchemaVersion:  2026.1
//  Description:    Deterministic request envelope scoping the GlobalSafetyInsights TRIR/LTIR KPI
//                  section for cross-module composition (Handbook Part IX §9.3.2). Owned by this
//                  submodule; contains no domain logic. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.GlobalSafetyInsights.Contracts;

/// <summary>Scopes the GlobalSafetyInsights TRIR/LTIR KPI section for cross-module composition.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="From">Inclusive UTC start of the history range considered.</param>
/// <param name="To">Inclusive UTC end of the history range considered.</param>
public sealed record GlobalSafetyKpiSectionRequest(Guid TenantId, DateTime From, DateTime To);
