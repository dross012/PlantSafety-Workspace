// ============================================================================
// File: ImportActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Integrations.Import endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ImportActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Integrations.Import-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Integrations.Import-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Import.Filters;

/// <summary>
/// Optional action filter for Integrations.Import. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ImportActionFilter : IActionFilter
{
    private readonly ILogger<ImportActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ImportActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ImportActionFilter(ILogger<ImportActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Integrations.Import action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
