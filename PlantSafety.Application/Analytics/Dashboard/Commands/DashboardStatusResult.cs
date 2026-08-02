// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           DashboardStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Dashboard lifecycle-transition commands
//                  (Publish, RefreshData, Deprecate, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Dashboard.Commands;

/// <summary>Result payload shared by Dashboard lifecycle-transition commands.</summary>
/// <param name="DashboardId">Identity of the affected Dashboard aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record DashboardStatusResult(Guid DashboardId, string Status);
