/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ModuleMarker.cs
    DESCRIPTION:        Marker type used by SafetyGenerator and SafetyAnalyzer to identify module assemblies for scanning.
                        MUST remain empty, sealed, and stable.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.CodeGeneration.Markers;

/// <summary>
/// Marker type used to identify module assemblies.
/// </summary>
public sealed class ModuleMarker;
