// ============================================================================
//  Project:        Deployment
//  Module:         Orchestration
//  File:           DeploymentEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The deterministic, ordered,
//                  idempotent step executor: steps run in ascending
//                  StepIndex order, each with its own timeout and retry
//                  budget; the first step that exhausts its retries stops
//                  the run (deterministic fail-fast, never a partial
//                  continue-past-failure), and every previously-succeeded
//                  step's compensation runs in reverse order. Each
//                  DeploymentStep's Action must itself be idempotent (safe
//                  to re-run on retry) -- DeploymentEngine enforces ordering/
//                  timeout/retry/compensation, it does not and cannot make a
//                  non-idempotent action idempotent.
// ============================================================================
using Deployment.Observability;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Deployment.Orchestration;

/// <summary>A single deployment step's action: returns true on success, false on a non-throwing failure.</summary>
public delegate Task<bool> DeploymentStepAction(CancellationToken cancellationToken);

/// <summary>A single deployment step's compensation (rollback) action, run if a later step fails.</summary>
public delegate Task DeploymentStepCompensation(CancellationToken cancellationToken);

/// <summary>A single, ordered, idempotent deployment step.</summary>
public sealed record DeploymentStep(
    string StepName,
    int StepIndex,
    TimeSpan Timeout,
    int MaxRetries,
    DeploymentStepAction Action,
    DeploymentStepCompensation? Compensation);

/// <summary>The deterministic outcome of a single step's execution (including retries).</summary>
public sealed record DeploymentStepResult(string StepName, bool Succeeded, int AttemptsUsed, string? FailureReason);

/// <summary>The deterministic outcome of a full deployment execution.</summary>
public sealed record DeploymentExecutionResult(
    string DeploymentId,
    IReadOnlyList<DeploymentStepResult> StepResults,
    bool Succeeded,
    bool CompensationRan);

/// <summary>The deterministic, ordered, idempotent deployment step executor.</summary>
public static class DeploymentEngine
{
    /// <summary>
    /// Executes <paramref name="steps"/> in ascending StepIndex order. Stops at the first step that
    /// exhausts its retry budget, then runs every previously-succeeded step's compensation in reverse
    /// order.
    /// </summary>
    public static async Task<DeploymentExecutionResult> ExecuteAsync(
        string deploymentId,
        IReadOnlyList<DeploymentStep> steps,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deploymentId);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        List<DeploymentStep> orderedSteps = steps.OrderBy(s => s.StepIndex).ToList();
        var results = new List<DeploymentStepResult>();
        var succeededSteps = new List<DeploymentStep>();
        bool allSucceeded = true;

        foreach (DeploymentStep step in orderedSteps)
        {
            DeploymentStepResult stepResult = await ExecuteStepWithRetryAsync(step, cancellationToken);
            results.Add(stepResult);
            DeploymentTelemetry.RecordStepExecuted(step.StepName, stepResult.Succeeded, clock, logEventWriter, correlationId);

            if (stepResult.Succeeded)
            {
                succeededSteps.Add(step);
            }
            else
            {
                allSucceeded = false;
                break;
            }
        }

        bool compensationRan = false;
        if (!allSucceeded)
        {
            for (int i = succeededSteps.Count - 1; i >= 0; i--)
            {
                DeploymentStep step = succeededSteps[i];
                if (step.Compensation is not null)
                {
                    await step.Compensation(cancellationToken);
                    compensationRan = true;
                }
            }
        }

        return new DeploymentExecutionResult(deploymentId, results, allSucceeded, compensationRan);
    }

    private static async Task<DeploymentStepResult> ExecuteStepWithRetryAsync(DeploymentStep step, CancellationToken cancellationToken)
    {
        int attempts = 0;
        string? lastFailureReason = null;

        while (attempts <= step.MaxRetries)
        {
            attempts++;

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(step.Timeout);

            try
            {
                bool succeeded = await step.Action(timeoutCts.Token);
                if (succeeded)
                {
                    return new DeploymentStepResult(step.StepName, true, attempts, null);
                }

                lastFailureReason = $"Step \"{step.StepName}\" action returned false on attempt {attempts}.";
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastFailureReason = $"Step \"{step.StepName}\" timed out after {step.Timeout} on attempt {attempts}.";
            }
            catch (Exception ex)
            {
                lastFailureReason = $"Step \"{step.StepName}\" threw on attempt {attempts}: {ex.Message}";
            }
        }

        return new DeploymentStepResult(step.StepName, false, attempts, lastFailureReason);
    }
}
