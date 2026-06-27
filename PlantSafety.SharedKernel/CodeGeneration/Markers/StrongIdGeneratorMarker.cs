/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               StrongIdGeneratorMarker.cs
    DESCRIPTION:        Marker type used by SafetyGenerator to locate StrongId generator templates. MUST remain empty,
                        sealed, and stable.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.CodeGeneration.Markers;

/// <summary>
/// Marker type used to locate StrongId generator templates.
/// </summary>
public sealed class StrongIdGeneratorMarker;
