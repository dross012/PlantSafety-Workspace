// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Presentation-only point for TrendChartComponent — a single
//   (Label, Value) pair plotted along the chart's horizontal axis. Built by
//   each module's page/state from its own item DTOs; carries no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

namespace PlantSafety.UI.Modules.DashboardAnalytics.Models;

/// <summary>A single plotted point on a <c>TrendChartComponent</c> (module-agnostic).</summary>
/// <param name="Label">Axis label for this point (e.g., a formatted timestamp or metric name).</param>
/// <param name="Value">The plotted numeric value.</param>
public sealed record TrendChartPoint(string Label, decimal Value);
