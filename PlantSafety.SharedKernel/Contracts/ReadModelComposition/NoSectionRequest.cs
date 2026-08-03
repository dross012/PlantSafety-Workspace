// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical, reusable empty request envelope for
//   IReportSectionProvider&lt;TRequest, TSection&gt; implementations whose section requires no
//   scoping parameters (e.g. a platform-wide health rollup with no tenant/entity filter). Avoids
//   every contributing submodule having to declare its own trivial parameterless marker type.
// SchemaVersion: 1
// Additive-only evolution: this type MUST remain an empty, side-effect-free marker.

namespace PlantSafety.SharedKernel.Contracts.ReadModelComposition;

/// <summary>
/// Canonical empty request envelope for <see cref="IReportSectionProvider{TRequest, TSection}"/>
/// sections that require no scoping parameters.
/// </summary>
public readonly record struct NoSectionRequest
{
    /// <summary>The single canonical instance. Reused to avoid unnecessary allocations.</summary>
    public static readonly NoSectionRequest Instance = default;
}
