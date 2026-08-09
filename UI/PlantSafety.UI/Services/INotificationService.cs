// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           INotificationService.cs
//  Created:        2026-08-09
//  Description:    Phase 2.2 deliverable: the unified notification service.
//                  Replaces ad-hoc, per-page transient feedback (there was
//                  previously no consistent way to confirm "Facility
//                  created" beyond the page navigating away) with one
//                  seam every module calls, rendered by the single
//                  NotificationHostComponent in MainLayout. Deliberately
//                  separate from ErrorComponent (Section 12: errors are
//                  never auto-dismissed or hidden) -- this service is for
//                  transient, dismissible, non-error feedback only.
// ============================================================================
namespace PlantSafety.UI.Services;

/// <summary>The severity of a transient UI notification.</summary>
public enum NotificationSeverity
{
    /// <summary>A neutral, informational notification.</summary>
    Info,

    /// <summary>A notification confirming a successful action.</summary>
    Success,

    /// <summary>A notification warning of a non-blocking issue.</summary>
    Warning,
}

/// <summary>One transient UI notification.</summary>
public sealed record Notification(Guid NotificationId, NotificationSeverity Severity, string Message);

/// <summary>The one approved seam for transient, dismissible, non-error UI feedback.</summary>
public interface INotificationService
{
    /// <summary>The currently active notifications, oldest first.</summary>
    IReadOnlyList<Notification> Active { get; }

    /// <summary>Raised whenever the active notification list changes.</summary>
    event Action? Changed;

    /// <summary>Shows an informational notification.</summary>
    void Info(string message);

    /// <summary>Shows a success notification.</summary>
    void Success(string message);

    /// <summary>Shows a warning notification.</summary>
    void Warning(string message);

    /// <summary>Dismisses the given notification, if still active.</summary>
    void Dismiss(Guid notificationId);
}
