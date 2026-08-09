// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           LoadingComponentTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for LoadingComponent: default/custom message,
//                  and the accessibility contract (role="status",
//                  aria-live="polite") every module page's Loading state
//                  depends on.
// ============================================================================
using Bunit;

using AwesomeAssertions;

using PlantSafety.UI.Shared.Components;

using Xunit;

namespace PlantSafety.UI.Tests.Shared.Components;

public sealed class LoadingComponentTests : TestContext
{
    [Fact]
    public void Render_NoMessageSupplied_ShowsDefaultText()
    {
        IRenderedComponent<LoadingComponent> component = RenderComponent<LoadingComponent>();

        component.Find(".ps-loading__text").TextContent.Should().Be("Loading...");
    }

    [Fact]
    public void Render_CustomMessage_ShowsSuppliedText()
    {
        IRenderedComponent<LoadingComponent> component = RenderComponent<LoadingComponent>(p => p.Add(c => c.Message, "Loading facilities..."));

        component.Find(".ps-loading__text").TextContent.Should().Be("Loading facilities...");
    }

    [Fact]
    public void Render_HasStatusRoleAndPoliteAriaLive()
    {
        IRenderedComponent<LoadingComponent> component = RenderComponent<LoadingComponent>();

        var root = component.Find(".ps-loading");
        root.GetAttribute("role").Should().Be("status");
        root.GetAttribute("aria-live").Should().Be("polite");
    }
}
