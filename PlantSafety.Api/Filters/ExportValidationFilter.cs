// ============================================================================
// File: ExportValidationFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: IActionFilter that validates ExportRequest parameters before
//              export actions execute. Enforces date range reasonability, MaxRecords
//              cap, and tenant-context presence.
// ============================================================================
//
// Onboarding Notes:
// - Applied per-action via [ServiceFilter(typeof(ExportValidationFilter))] on
//   each export controller's ExportAsync method. This is preferable to global
//   registration because ExportValidationFilter is only relevant on export actions.
// - Registers ExportValidationFilter as a scoped service in ServiceCollectionExtensions
//   so [ServiceFilter] can resolve it.
// - Validation rules enforced:
//     1. TenantId must be non-empty Guid.
//     2. From must be before To (when both are supplied).
//     3. Date range must not exceed 2 years (prevents unbounded exports).
//     4. MaxRecords is silently clamped to 50,000 (not a validation error; just bounded).
//     5. Format must be a defined ExportFormat enum value.
// - Returns HTTP 422 ValidationProblemDetails on failure, consistent with
//   ValidationProblemDetailsFilter.
//
// Rationale:
// - Export endpoints are the highest-cost operations in the system (full table
//   scans, file generation, Blob upload). Validating eagerly prevents wasted work
//   on invalid requests before they reach the Application layer.
//
// Future Extension Seams:
// - Add per-tenant concurrent export limit check (max 3 simultaneous exports
//   per tenant) using IMemoryCache before dispatching to the Application layer.
//
// ============================================================================

using PlantSafety.Api.Models.Exports;

namespace PlantSafety.Api.Filters;

/// <summary>
/// Action filter that validates <see cref="ExportRequest"/> parameters on all
/// export controller actions. Returns HTTP 422 for invalid inputs.
/// </summary>
public sealed class ExportValidationFilter : IActionFilter
{
    private const int MaxRecordsCap  = 50_000;
    private static readonly TimeSpan MaxDateRange = TimeSpan.FromDays(365 * 2);

    private readonly ILogger<ExportValidationFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ExportValidationFilter"/>.</summary>
    public ExportValidationFilter(ILogger<ExportValidationFilter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.ActionArguments.Values
            .OfType<ExportRequest>()
            .FirstOrDefault();

        if (request is null)
            return;

        var errors = new Dictionary<string, string[]>();

        if (request.TenantId == Guid.Empty)
            errors["TenantId"] = ["TenantId must be a non-empty GUID."];

        if (request.From.HasValue && request.To.HasValue && request.From >= request.To)
            errors["From"] = ["From must be earlier than To."];

        if (request.From.HasValue && request.To.HasValue)
        {
            var range = (request.To.Value - request.From.Value);
            if (range > MaxDateRange)
                errors["DateRange"] = [$"Date range must not exceed {(int)MaxDateRange.TotalDays / 365} years."];
        }

        if (!Enum.IsDefined(request.Format))
            errors["Format"] = [$"Format must be one of: {string.Join(", ", Enum.GetNames<ExportFormat>())}."];

        if (errors.Count > 0)
        {
            var problem = new ValidationProblemDetails(errors)
            {
                Type   = "https://tools.ietf.org/html/rfc7807#validation",
                Title  = "Export request validation failed.",
                Status = StatusCodes.Status422UnprocessableEntity
            };
            context.Result = new UnprocessableEntityObjectResult(problem)
            {
                ContentTypes = { "application/problem+json" }
            };
            return;
        }

        // Silently clamp MaxRecords — not an error, just a guard
        if (request.MaxRecords > MaxRecordsCap)
        {
            _logger.LogInformation(
                "MaxRecords {Requested} clamped to {Cap} for TenantId={TenantId}",
                request.MaxRecords, MaxRecordsCap, request.TenantId);
            request.MaxRecords = MaxRecordsCap;
        }
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context) { }
}
