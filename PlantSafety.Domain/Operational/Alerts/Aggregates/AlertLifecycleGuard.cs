// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertLifecycleGuard.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  Description:    Centralized lifecycle guard for the Alert aggregate,
//                  enforcing the canonical state machine defined in the
//                  Alerts Module Playbook. All lifecycle transitions are
//                  validated here to ensure deterministic behavior.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Aggregates
{
    /// <summary>
    /// <para>
    /// Provides centralized guard methods for validating lifecycle transitions
    /// of the <see cref="Alert"/> aggregate.
    /// </para>
    /// <para>
    /// This class encodes the canonical lifecycle:
    /// Created → Acknowledged → InProgress → Escalated → Resolved → Closed → Archived.
    /// </para>
    /// <para>
    /// All domain methods on <see cref="Alert"/> must delegate lifecycle checks
    /// to this guard to ensure a single source of truth for transition rules.
    /// </para>
    /// </summary>
    internal static class AlertLifecycleGuard
    {
        /// <summary>
        /// Ensures that an alert in the specified <paramref name="currentStatus"/>
        /// can be acknowledged.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the transition is not allowed.
        /// </exception>
        public static void EnsureCanAcknowledge(AlertStatus currentStatus)
        {
            if (currentStatus != AlertStatus.Created)
            {
                throw new InvalidOperationException(
                    $"Alert can only be acknowledged from '{AlertStatus.Created}', " +
                    $"but current status is '{currentStatus}'.");
            }
        }

        /// <summary>
        /// Ensures that an alert in the specified <paramref name="currentStatus"/>
        /// can be moved to InProgress.
        /// </summary>
        public static void EnsureCanStartWork(AlertStatus currentStatus)
        {
            if (currentStatus != AlertStatus.Acknowledged)
            {
                throw new InvalidOperationException(
                    $"Alert work can only be started from '{AlertStatus.Acknowledged}', " +
                    $"but current status is '{currentStatus}'.");
            }
        }

        /// <summary>
        /// Ensures that an alert in the specified <paramref name="currentStatus"/>
        /// can be escalated.
        /// </summary>
        public static void EnsureCanEscalate(AlertStatus currentStatus)
        {
            if (currentStatus != AlertStatus.InProgress &&
                currentStatus != AlertStatus.Acknowledged)
            {
                throw new InvalidOperationException(
                    $"Alert can only be escalated from '{AlertStatus.Acknowledged}' " +
                    $"or '{AlertStatus.InProgress}', but current status is '{currentStatus}'.");
            }
        }

        /// <summary>
        /// Ensures that an alert in the specified <paramref name="currentStatus"/>
        /// can be resolved.
        /// </summary>
        public static void EnsureCanResolve(AlertStatus currentStatus)
        {
            if (currentStatus != AlertStatus.InProgress &&
                currentStatus != AlertStatus.Escalated)
            {
                throw new InvalidOperationException(
                    $"Alert can only be resolved from '{AlertStatus.InProgress}' " +
                    $"or '{AlertStatus.Escalated}', but current status is '{currentStatus}'.");
            }
        }

        /// <summary>
        /// Ensures that an alert in the specified <paramref name="currentStatus"/>
        /// can be closed.
        /// </summary>
        public static void EnsureCanClose(AlertStatus currentStatus)
        {
            if (currentStatus != AlertStatus.Resolved)
            {
                throw new InvalidOperationException(
                    $"Alert can only be closed from '{AlertStatus.Resolved}', " +
                    $"but current status is '{currentStatus}'.");
            }
        }

        /// <summary>
        /// Ensures that an alert in the specified <paramref name="currentStatus"/>
        /// can be archived.
        /// </summary>
        public static void EnsureCanArchive(AlertStatus currentStatus)
        {
            if (currentStatus != AlertStatus.Closed)
            {
                throw new InvalidOperationException(
                    $"Alert can only be archived from '{AlertStatus.Closed}', " +
                    $"but current status is '{currentStatus}'.");
            }
        }
    }
}

