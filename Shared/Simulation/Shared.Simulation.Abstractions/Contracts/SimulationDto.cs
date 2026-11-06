// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Contracts
//  File:           SimulationDto.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Flat, serialization-friendly DTOs
//                  projected from SimulationEvaluationResultContract for
//                  API/UI consumption -- deterministic mapping only.
// ============================================================================
using Shared.Simulation.Abstractions.Scenario;
using Shared.Simulation.Abstractions.Simulation;

namespace Shared.Simulation.Abstractions.Contracts;

/// <summary>Flat DTO projection of a single step's evaluation outcome.</summary>
public sealed record SimulationStepOutcomeDto(int StepIndex, string StepType, bool RequiresEscalation, string Priority);

/// <summary>Flat DTO projection of a SimulationEvaluationResultContract.</summary>
public sealed record SimulationEvaluationDto(
    string ScenarioId,
    bool IsValidReplay,
    IReadOnlyList<SimulationStepOutcomeDto> StepOutcomes,
    bool RequiresOverallEscalation,
    string OverallPriority);

/// <summary>Deterministic, pure mapping from a SimulationEvaluationResultContract to its DTO projection.</summary>
public static class SimulationDtoMapper
{
    /// <summary>Projects <paramref name="result"/> into a SimulationEvaluationDto.</summary>
    public static SimulationEvaluationDto ToDto(SimulationEvaluationResultContract result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var stepOutcomes = new List<SimulationStepOutcomeDto>(result.Outcome.Scenario.StepOutcomes.Count);
        foreach (SimulationStepOutcome stepOutcome in result.Outcome.Scenario.StepOutcomes)
        {
            stepOutcomes.Add(new SimulationStepOutcomeDto(
                stepOutcome.StepIndex,
                stepOutcome.StepType.ToString(),
                stepOutcome.RequiresEscalation,
                stepOutcome.Priority.ToString()));
        }

        return new SimulationEvaluationDto(
            ScenarioId: result.Outcome.Scenario.ScenarioId,
            IsValidReplay: result.Outcome.Replay.IsValidReplay,
            StepOutcomes: stepOutcomes,
            RequiresOverallEscalation: result.Outcome.RequiresOverallEscalation,
            OverallPriority: result.Outcome.OverallPriority.ToString());
    }
}
