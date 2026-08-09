// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           NotificationServiceTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using PlantSafety.UI.Services;

using Xunit;

namespace PlantSafety.UI.Tests.Services;

public sealed class NotificationServiceTests
{
    [Fact]
    public void Success_AddsAnActiveNotificationWithSuccessSeverity()
    {
        INotificationService service = new NotificationService();

        service.Success("Facility created.");

        service.Active.Should().ContainSingle(n => n.Severity == NotificationSeverity.Success && n.Message == "Facility created.");
    }

    [Fact]
    public void Info_AndWarning_AddDistinctSeverities()
    {
        INotificationService service = new NotificationService();

        service.Info("info message");
        service.Warning("warning message");

        service.Active.Should().Contain(n => n.Severity == NotificationSeverity.Info);
        service.Active.Should().Contain(n => n.Severity == NotificationSeverity.Warning);
    }

    [Fact]
    public void Dismiss_RemovesOnlyTheMatchingNotification()
    {
        INotificationService service = new NotificationService();
        service.Success("first");
        service.Success("second");
        Guid firstId = service.Active[0].NotificationId;

        service.Dismiss(firstId);

        service.Active.Should().ContainSingle(n => n.Message == "second");
    }

    [Fact]
    public void Changed_IsRaisedWhenANotificationIsAddedOrDismissed()
    {
        INotificationService service = new NotificationService();
        var changeCount = 0;
        service.Changed += () => changeCount++;

        service.Success("first");
        service.Dismiss(service.Active[0].NotificationId);

        changeCount.Should().Be(2);
    }

    [Fact]
    public void Dismiss_UnknownId_DoesNotRaiseChanged()
    {
        INotificationService service = new NotificationService();
        var changeCount = 0;
        service.Changed += () => changeCount++;

        service.Dismiss(Guid.NewGuid());

        changeCount.Should().Be(0);
    }
}
