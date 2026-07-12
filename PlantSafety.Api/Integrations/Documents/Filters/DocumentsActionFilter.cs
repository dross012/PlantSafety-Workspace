// ============================================================================
// File: DocumentsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Integrations.Documents endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(DocumentsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Integrations.Documents-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Integrations.Documents-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Documents.Filters;

/// <summary>
/// Optional action filter for Integrations.Documents. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class DocumentsActionFilter : IActionFilter
{
    private readonly ILogger<DocumentsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="DocumentsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public DocumentsActionFilter(ILogger<DocumentsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Integrations.Documents action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
