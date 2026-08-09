// ============================================================================
//  Project:        AI.DigitalTwinLearning
//  Module:         Abstractions.Learning
//  File:           DigitalTwinLearningOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.7: the result of a digital-twin learning
//                  session -- one LearnedPattern per category, and every
//                  matched DigitalTwinLearningRule's model-update
//                  recommendation. ModelUpdateRecommendations are the
//                  deterministic feedback this engine produces for
//                  "updating digital twin models (D)" -- applying a
//                  recommendation to a real Enterprise.DigitalTwin
//                  snapshot is a real host's job, scoped out here the same
//                  way Path B's NullFacilityInferenceResolver deliberately
//                  scoped out real tenant-facility binding.
// ============================================================================
namespace AI.DigitalTwinLearning.Abstractions.Learning;

/// <summary>The result of a digital-twin learning session.</summary>
public sealed record DigitalTwinLearningOutput(
    IReadOnlyList<LearnedPattern> Patterns,
    IReadOnlyList<string> ModelUpdateRecommendations,
    DateTime GeneratedAtUtc);
