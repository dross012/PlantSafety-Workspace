// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Contracts
//  File:           SimulationErrors.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Deterministic Simulation error
//                  codes and the one validation exception it raises.
//                  Derives from ArgumentException, same reasoning
//                  SchedulingErrors.cs/PredictiveSafetyErrors.cs/
//                  SafetyGraphErrors.cs document: Shared.ErrorHandling's
//                  existing ExceptionClassifier already maps
//                  ArgumentException -> ErrorCategory.Validation -> HTTP
//                  400, reused rather than reinvented.
// ============================================================================
namespace Shared.Simulation.Abstractions.Contracts;

/// <summary>Deterministic Simulation error codes.</summary>
public static class SimulationErrors
{
    /// <summary>Raised when a simulation evaluation is requested with a scenario carrying zero steps.</summary>
    public const string EmptyScenario = "SIMULATION_EMPTY_SCENARIO";

    /// <summary>Raised when a SimulationMetadata carries an unrecognized SchemaVersion.</summary>
    public const string UnsupportedSchemaVersion = "SIMULATION_UNSUPPORTED_SCHEMA_VERSION";
}

/// <summary>The one validation exception Simulation raises for a deterministically invalid evaluation request.</summary>
public sealed class SimulationValidationException : ArgumentException
{
    /// <summary>The deterministic SimulationErrors code identifying the failed validation.</summary>
    public string ErrorCode { get; }

    /// <summary>Constructs a SimulationValidationException for <paramref name="errorCode"/>.</summary>
    public SimulationValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
