// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           IClock.cs
//  Created:        2026-08-09
//  Description:    A UI-local IClock, distinct from (not a reference to)
//                  Shared.Deterministic.Abstractions.Clock.IClock --
//                  PlantSafety.UI takes NO project reference to backend
//                  projects (Section 10, the same rule that makes
//                  FacilityBackendDto a local mirror rather than a shared
//                  type). Used only to make ThrottleGuard's time-window
//                  logic deterministically testable; not tied to backend
//                  determinism at all.
// ============================================================================
namespace PlantSafety.UI.Services;

/// <summary>A UI-local seam for reading the current UTC time, kept separate from any backend project.</summary>
public interface IClock
{
    /// <summary>The current UTC instant.</summary>
    DateTime UtcNow { get; }
}
