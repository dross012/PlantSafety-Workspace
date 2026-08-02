// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           CreateDashboardResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateDashboardCommand. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Dashboard.Commands;

/// <summary>Result of a successful <see cref="CreateDashboardCommand"/> execution.</summary>
/// <param name="DashboardId">Identity of the newly created Dashboard aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateDashboardResult(
    Guid DashboardId,
    string Status);
