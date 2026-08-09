// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           NotificationService.cs
//  Created:        2026-08-09
//  Description:    In-memory, Scoped (one per Blazor circuit/session)
//                  implementation of INotificationService.
// ============================================================================
namespace PlantSafety.UI.Services;

/// <summary>In-memory, per-session notification service.</summary>
public sealed class NotificationService : INotificationService
{
    private readonly List<Notification> _active = [];

    /// <inheritdoc />
    public IReadOnlyList<Notification> Active => _active;

    /// <inheritdoc />
    public event Action? Changed;

    /// <inheritdoc />
    public void Info(string message) => Add(NotificationSeverity.Info, message);

    /// <inheritdoc />
    public void Success(string message) => Add(NotificationSeverity.Success, message);

    /// <inheritdoc />
    public void Warning(string message) => Add(NotificationSeverity.Warning, message);

    /// <inheritdoc />
    public void Dismiss(Guid notificationId)
    {
        int removed = _active.RemoveAll(n => n.NotificationId == notificationId);
        if (removed > 0)
        {
            Changed?.Invoke();
        }
    }

    private void Add(NotificationSeverity severity, string message)
    {
        _active.Add(new Notification(Guid.NewGuid(), severity, message));
        Changed?.Invoke();
    }
}
