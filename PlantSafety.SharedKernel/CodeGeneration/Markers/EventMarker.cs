/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               EventMarker.cs
    DESCRIPTION:        Marker type used by event catalog generators and SafetyGenerator to locate domain event assemblies.
                        MUST remain empty, sealed, and stable.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.CodeGeneration.Markers;

/// <summary>
/// Marker type used by event catalog generators.
/// </summary>
public sealed class EventMarker;
