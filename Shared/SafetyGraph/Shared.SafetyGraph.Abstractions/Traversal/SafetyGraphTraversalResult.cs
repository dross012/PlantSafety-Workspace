// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Traversal
//  File:           SafetyGraphTraversalResult.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The deterministic output shape
//                  every SafetyGraphTraversalEngine method returns.
//                  VisitedNodeIds/EnumeratedPaths/CycleNodeIds are always in
//                  the same order for the same graph and start node --
//                  never dependent on dictionary/set iteration order (the
//                  engine sorts adjacency deterministically before visiting;
//                  see SafetyGraphTraversalRules.BuildAdjacency).
// ============================================================================
namespace Shared.SafetyGraph.Abstractions.Traversal;

/// <summary>The deterministic result of a single graph traversal operation.</summary>
public sealed record SafetyGraphTraversalResult(
    IReadOnlyList<string> VisitedNodeIds,
    IReadOnlyList<IReadOnlyList<string>> EnumeratedPaths,
    bool CycleDetected,
    IReadOnlyList<string> CycleNodeIds);
