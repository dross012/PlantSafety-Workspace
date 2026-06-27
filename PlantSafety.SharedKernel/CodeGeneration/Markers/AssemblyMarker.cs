/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               AssemblyMarker.cs
    DESCRIPTION:        Marker type used by SafetyGenerator, SafetyAnalyzer, and SafetyBuild to locate the SharedKernel
                        assembly. MUST remain empty, sealed, and stable. MUST NOT contain logic or behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. This type MUST NOT be renamed or removed.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.CodeGeneration.Markers;

/// <summary>
/// Marker type used to locate the SharedKernel assembly.
/// </summary>
public sealed class AssemblyMarker;
