// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           EmptyComponentTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for EmptyComponent: default/custom message.
// ============================================================================
using Bunit;

using AwesomeAssertions;

using PlantSafety.UI.Shared.Components;

using Xunit;

namespace PlantSafety.UI.Tests.Shared.Components;

public sealed class EmptyComponentTests : TestContext
{
    [Fact]
    public void Render_NoMessageSupplied_ShowsDefaultText()
    {
        IRenderedComponent<EmptyComponent> component = RenderComponent<EmptyComponent>();

        component.Find(".ps-empty p").TextContent.Should().Be("No data yet.");
    }

    [Fact]
    public void Render_CustomMessage_ShowsSuppliedText()
    {
        IRenderedComponent<EmptyComponent> component = RenderComponent<EmptyComponent>(p => p.Add(c => c.Message, "No facilities yet."));

        component.Find(".ps-empty p").TextContent.Should().Be("No facilities yet.");
    }
}
