// ============================================================================
// File: ApiConventions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: ASP.NET Core API conventions applied globally to all PlantSafety.Api
//              controllers. Declares the canonical HTTP response codes for standard
//              action patterns (Get, GetById, Create, Update, Delete) to keep
//              Swashbuckle OpenAPI output accurate without per-action [ProducesResponseType]
//              decoration on all 121 controllers.
// ============================================================================
//
// Onboarding Notes:
// - Applied globally in ServiceCollectionExtensions via
//   options.Conventions.Add(new ApiExplorerGroupPerVersionConvention()) and
//   [assembly: ApiConventionType(typeof(PlantSafetyApiConventions))] in the
//   project's AssemblyInfo or in Controllers.BaseApiController.
// - Each convention method matches an action by naming pattern and parameter
//   types. The [ApiConventionMethod] attribute on module controllers overrides
//   the global convention for specific actions.
// - All response types use object as the payload type — Swashbuckle infers the
//   actual type from the IActionResult return (requires ProducesAttribute or
//   generic return type).
// - Adding a new action pattern? Follow the naming and parameter conventions here
//   (GetAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync) to get
//   automatic OpenAPI documentation.
//
// Rationale:
// - Eliminates 500+ [ProducesResponseType] attribute repetitions across module
//   controllers while keeping the OpenAPI spec accurate.
//
// Future Extension Seams:
// - Add ApproveAsync convention (200, 400, 404, 409) for workflow approval endpoints.
// - Add ExportAsync convention (200 File, 400, 404) for export endpoints.
//
// ============================================================================

using Microsoft.AspNetCore.Mvc.ApiExplorer;

[assembly: ApiConventionType(typeof(PlantSafety.Api.Conventions.PlantSafetyApiConventions))]

namespace PlantSafety.Api.Conventions;

/// <summary>
/// Global API conventions for PlantSafety.Api. Declares standard HTTP response
/// codes for common action patterns, eliminating per-action ProducesResponseType
/// attributes across all module controllers.
/// </summary>
public static class PlantSafetyApiConventions
{
    /// <summary>Convention for paginated list GET actions (e.g. GetListAsync).</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void GetList(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object request)
    { }

    /// <summary>Convention for single-resource GET actions (e.g. GetByIdAsync).</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void GetById(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id)
    { }

    /// <summary>Convention for resource creation POST actions (e.g. CreateAsync).</summary>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Create(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object request)
    { }

    /// <summary>Convention for resource update PUT actions (e.g. UpdateAsync).</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Update(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object request)
    { }

    /// <summary>Convention for resource deletion DELETE actions (e.g. DeleteAsync).</summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Delete(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id)
    { }
}
