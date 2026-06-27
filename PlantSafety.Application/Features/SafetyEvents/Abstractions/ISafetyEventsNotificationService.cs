// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the SafetyEvents module.
//   These interfaces are Application-layer contracts; Infrastructure provides the implementations.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Abstractions;

/// <summary>Sends real-time notifications for high-severity safety events.</summary>
public interface ISafetyEventsNotificationService
{
    /// <summary>Sends an immediate alert for a critical or crisis-level event.</summary>
    Task SendCriticalAlertAsync(
        Guid tenantId,
        Guid siteId,
        string eventType,
        Guid eventId,
        string title,
        string severity,
        CancellationToken cancellationToken);
}

/// <summary>Provides severity classification helpers for safety events.</summary>
public interface ISafetyEventClassificationService
{
    /// <summary>
    /// Returns the computed severity class for an incident given its reported attributes.
    /// Delegates to the Intelligence module; returns <c>null</c> when classification is unavailable.
    /// </summary>
    Task<string?> ClassifyAsync(
        Guid tenantId,
        string description,
        string location,
        CancellationToken cancellationToken);
}
