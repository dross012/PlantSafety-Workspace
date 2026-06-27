// ============================================================================
// File: DocumentsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Documents endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(DocumentsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Documents-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Documents-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.Documents.Filters;

/// <summary>
/// Optional action filter for Documents. Applies sub-area-specific request
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
        _logger.LogDebug("Documents action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}