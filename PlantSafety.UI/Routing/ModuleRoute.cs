// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Metadata descriptor for a single navigable module.
//   Used by NavMenu to build the module navigation tree without magic strings.
//   All 66 modules are registered in ModuleRouteRegistry.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

namespace PlantSafety.UI.Routing;

/// <summary>
/// Describes a single navigable module entry for the nav menu.
/// All string fields are non-nullable; icon names use Bootstrap Icons identifiers.
/// </summary>
public sealed class ModuleRoute
{
    /// <summary>Gets the module code matching the TenantDto.EnabledModules list.</summary>
    public string ModuleCode { get; init; } = string.Empty;

    /// <summary>Gets the display label shown in the nav menu.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Gets the strongly-typed route href from <see cref="RouteDefinitions"/>.</summary>
    public string Href { get; init; } = string.Empty;

    /// <summary>Gets the Bootstrap Icons class name for the nav icon (e.g., "bi-bell").</summary>
    public string IconClass { get; init; } = string.Empty;

    /// <summary>Gets the nav section this module belongs to (matches the v5.0 section name).</summary>
    public string Section { get; init; } = string.Empty;

    /// <summary>Gets the display order within the section.</summary>
    public int SectionOrder { get; init; }
}
