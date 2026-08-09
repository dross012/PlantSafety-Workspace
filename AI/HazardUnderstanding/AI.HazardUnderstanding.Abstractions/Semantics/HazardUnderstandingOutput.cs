// ============================================================================
//  Project:        AI.HazardUnderstanding
//  Module:         Abstractions.Semantics
//  File:           HazardUnderstandingOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.2: the result of a hazard-understanding
//                  session -- the full HazardSemanticModel plus the
//                  timestamp it was generated at.
// ============================================================================
namespace AI.HazardUnderstanding.Abstractions.Semantics;

/// <summary>The result of a hazard-understanding session.</summary>
public sealed record HazardUnderstandingOutput(
    HazardSemanticModel Model,
    DateTime GeneratedAtUtc);
