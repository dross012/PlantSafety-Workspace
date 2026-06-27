// ============================================================================
// File: KPISnapshotsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for KPISnapshots endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(KPISnapshotsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows KPISnapshots-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for KPISnapshots-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Dashboard.KPISnapshots.Filters;

/// <summary>
/// Optional action filter for KPISnapshots. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class KPISnapshotsActionFilter : IActionFilter
{
    private readonly ILogger<KPISnapshotsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="KPISnapshotsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public KPISnapshotsActionFilter(ILogger<KPISnapshotsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("KPISnapshots action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}