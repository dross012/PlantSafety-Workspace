// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           SystemClock.cs
//  Created:        2026-08-09
//  Description:    The production UI-local IClock: a thin wrapper over
//                  the real wall clock.
// ============================================================================
namespace PlantSafety.UI.Services;

/// <summary>The production UI-local clock: reads the real system UTC time.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
