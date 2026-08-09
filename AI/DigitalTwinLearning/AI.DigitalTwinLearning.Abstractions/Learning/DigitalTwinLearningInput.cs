// ============================================================================
//  Project:        AI.DigitalTwinLearning
//  Module:         Abstractions.Learning
//  File:           DigitalTwinLearningInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.7 (Path E: AI-Driven Safety Automation): the
//                  input DigitalTwinLearningEngine.Learn reasons over --
//                  one historical occurrence-count series per pattern
//                  category Path E's spec requires the engine to learn.
// ============================================================================
namespace AI.DigitalTwinLearning.Abstractions.Learning;

/// <summary>The input a digital-twin learning session reasons over.</summary>
public sealed record DigitalTwinLearningInput(
    IReadOnlyList<int> HazardPropagationOccurrences,
    IReadOnlyList<int> SeverityEscalationOccurrences,
    IReadOnlyList<int> WorkflowBottleneckOccurrences,
    IReadOnlyList<int> EmployeeExposureOccurrences,
    IReadOnlyList<int> MachineFailureOccurrences,
    int WindowSize);
