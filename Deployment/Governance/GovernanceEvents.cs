// ============================================================================
//  Project:        Deployment
//  Module:         Governance
//  File:           GovernanceEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The fixed set of EventId literals
//                  Deployment's telemetry, audit, and governance layers
//                  write, mirroring every prior phase's *GovernanceEvents
//                  file exactly.
// ============================================================================
namespace Deployment.Governance;

/// <summary>The fixed set of EventId literals Deployment's telemetry, audit, and governance layers write.</summary>
public static class GovernanceEvents
{
    /// <summary>Emitted by DeploymentTelemetry after a single deployment step executes.</summary>
    public const string StepExecuted = "DEPLOYMENT_STEP_EXECUTED";

    /// <summary>Emitted by DeploymentTelemetry after a config snapshot is published.</summary>
    public const string SnapshotPublished = "DEPLOYMENT_SNAPSHOT_PUBLISHED";

    /// <summary>Emitted by DriftReconciler after a reconciliation pass completes.</summary>
    public const string DriftReconciled = "DEPLOYMENT_DRIFT_RECONCILED";

    /// <summary>Emitted by AuditLog for every immutable audit entry appended.</summary>
    public const string AuditEntryAppended = "DEPLOYMENT_AUDIT_ENTRY_APPENDED";

    /// <summary>Emitted by DeploymentGovernanceEvaluator when a governance rule violation is found.</summary>
    public const string GovernanceViolation = "DEPLOYMENT_GOVERNANCE_VIOLATION";

    /// <summary>Emitted by DeploymentGovernanceEvaluator when a deployment is blocked outright.</summary>
    public const string DeploymentBlocked = "DEPLOYMENT_BLOCKED";

    /// <summary>Emitted once a full deployment orchestration completes.</summary>
    public const string OrchestrationCompleted = "DEPLOYMENT_ORCHESTRATION_COMPLETED";

    /// <summary>Emitted by DeploymentTracing for each recorded span.</summary>
    public const string TraceSpan = "DEPLOYMENT_TRACE_SPAN";
}
