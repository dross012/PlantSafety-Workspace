/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               DomainModelMarker.cs
    DESCRIPTION:        Marker type used by SafetyGenerator to locate domain model assemblies for StrongId, ValueObject,
                        and DomainEvent generation. MUST remain empty and sealed.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.CodeGeneration.Markers;

/// <summary>
/// Marker type used to locate domain model assemblies.
/// </summary>
public sealed class DomainModelMarker;
