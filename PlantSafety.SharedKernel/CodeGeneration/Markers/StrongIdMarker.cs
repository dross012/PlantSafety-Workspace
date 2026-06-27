/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               StrongIdMarker.cs
    DESCRIPTION:        Marker type used by StrongId analyzers and SafetyGenerator to locate StrongId definitions. MUST
                        remain empty, sealed, and stable.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.CodeGeneration.Markers;

/// <summary>
/// Marker type used to locate StrongId definitions.
/// </summary>
public sealed class StrongIdMarker;
