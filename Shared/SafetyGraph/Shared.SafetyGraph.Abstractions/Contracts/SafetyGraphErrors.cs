// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Contracts
//  File:           SafetyGraphErrors.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministic Safety Graph error
//                  codes and the one validation exception it raises.
//                  Derives from ArgumentException, same reasoning
//                  SchedulingErrors.cs/PredictiveSafetyErrors.cs document:
//                  Shared.ErrorHandling's existing ExceptionClassifier
//                  already maps ArgumentException -> ErrorCategory.Validation
//                  -> HTTP 400, reused rather than reinvented.
// ============================================================================
namespace Shared.SafetyGraph.Abstractions.Contracts;

/// <summary>Deterministic Safety Graph error codes.</summary>
public static class SafetyGraphErrors
{
    /// <summary>Raised when a graph construction is requested with an empty facility name.</summary>
    public const string MissingFacilityName = "SAFETY_GRAPH_MISSING_FACILITY_NAME";

    /// <summary>Raised when a SafetyGraphMetadata carries an unrecognized SchemaVersion.</summary>
    public const string UnsupportedSchemaVersion = "SAFETY_GRAPH_UNSUPPORTED_SCHEMA_VERSION";

    /// <summary>Raised when an edge references a SourceNodeId or TargetNodeId not present in the graph's Nodes.</summary>
    public const string DanglingEdgeReference = "SAFETY_GRAPH_DANGLING_EDGE_REFERENCE";
}

/// <summary>The one validation exception Safety Graph raises for a deterministically invalid request.</summary>
public sealed class SafetyGraphValidationException : ArgumentException
{
    /// <summary>The deterministic SafetyGraphErrors code identifying the failed validation.</summary>
    public string ErrorCode { get; }

    /// <summary>Constructs a SafetyGraphValidationException for <paramref name="errorCode"/>.</summary>
    public SafetyGraphValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
