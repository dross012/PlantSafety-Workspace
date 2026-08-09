// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           NotificationHostComponentTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for NotificationHostComponent (Phase 2.2).
// ============================================================================
using AwesomeAssertions;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using PlantSafety.UI.Services;
using PlantSafety.UI.Shared.Components;

using Xunit;

namespace PlantSafety.UI.Tests.Shared.Components;

public sealed class NotificationHostComponentTests : TestContext
{
    [Fact]
    public void Render_NoActiveNotifications_RendersNothing()
    {
        Services.AddSingleton<INotificationService>(new NotificationService());

        IRenderedComponent<NotificationHostComponent> component = RenderComponent<NotificationHostComponent>();

        component.FindAll(".ps-notification").Should().BeEmpty();
    }

    [Fact]
    public void Render_ActiveSuccessNotification_RendersItWithSeverityClass()
    {
        var notifications = new NotificationService();
        notifications.Success("Facility created.");
        Services.AddSingleton<INotificationService>(notifications);

        IRenderedComponent<NotificationHostComponent> component = RenderComponent<NotificationHostComponent>();

        component.Find(".ps-notification--success .ps-notification__message").TextContent.Should().Be("Facility created.");
    }

    [Fact]
    public void ClickDismiss_RemovesTheNotificationFromTheHost()
    {
        var notifications = new NotificationService();
        notifications.Success("Facility created.");
        Services.AddSingleton<INotificationService>(notifications);
        IRenderedComponent<NotificationHostComponent> component = RenderComponent<NotificationHostComponent>();

        component.Find(".ps-notification__dismiss").Click();

        component.FindAll(".ps-notification").Should().BeEmpty();
    }
}
