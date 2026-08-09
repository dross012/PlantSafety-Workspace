// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           EngineConsoleComponentTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for EngineConsoleComponent -- the shared
//                  "run this engine" console every AI.*/Enterprise.* page
//                  is built from. Covers title/description rendering, two-
//                  way InputJson binding, the Run button raising OnRun with
//                  the current input, the busy state disabling Run, and
//                  conditional error/result panels.
// ============================================================================
using Bunit;

using AwesomeAssertions;

using PlantSafety.UI.Shared.Components;

using Xunit;

namespace PlantSafety.UI.Tests.Shared.Components;

public sealed class EngineConsoleComponentTests : TestContext
{
    [Fact]
    public void Render_ShowsTitleAndDescription()
    {
        IRenderedComponent<EngineConsoleComponent> component = RenderComponent<EngineConsoleComponent>(p => p
            .Add(c => c.Title, "Assess Severity")
            .Add(c => c.Description, "Infers base severity from hazard/injury/failure/chain context."));

        component.Find("h2").TextContent.Should().Be("Assess Severity");
        component.Find(".ps-engine-console__description").TextContent.Should().Be("Infers base severity from hazard/injury/failure/chain context.");
    }

    [Fact]
    public void Render_TextareaShowsCurrentInputJson()
    {
        IRenderedComponent<EngineConsoleComponent> component = RenderComponent<EngineConsoleComponent>(p => p
            .Add(c => c.Title, "T")
            .Add(c => c.InputJson, "{\"hazardType\":\"Fire\"}"));

        component.Find("textarea").GetAttribute("value").Should().Be("{\"hazardType\":\"Fire\"}");
    }

    [Fact]
    public void ClickRun_InvokesOnRunWithCurrentInputJson()
    {
        string? runWith = null;
        IRenderedComponent<EngineConsoleComponent> component = RenderComponent<EngineConsoleComponent>(p => p
            .Add(c => c.Title, "T")
            .Add(c => c.InputJson, "{\"a\":1}")
            .Add(c => c.OnRun, (string json) => runWith = json));

        component.Find(".ps-engine-console__run").Click();

        runWith.Should().Be("{\"a\":1}");
    }

    [Fact]
    public void Render_IsBusyTrue_RunButtonDisabledAndShowsRunningText()
    {
        IRenderedComponent<EngineConsoleComponent> component = RenderComponent<EngineConsoleComponent>(p => p
            .Add(c => c.Title, "T")
            .Add(c => c.IsBusy, true));

        var button = component.Find(".ps-engine-console__run");
        button.GetAttribute("disabled").Should().NotBeNull();
        button.TextContent.Trim().Should().Be("Running...");
    }

    [Fact]
    public void Render_ErrorMessageSupplied_RendersErrorComponent()
    {
        IRenderedComponent<EngineConsoleComponent> component = RenderComponent<EngineConsoleComponent>(p => p
            .Add(c => c.Title, "T")
            .Add(c => c.ErrorMessage, "Something went wrong."));

        component.Find(".ps-error__message").TextContent.Should().Be("Something went wrong.");
    }

    [Fact]
    public void Render_NoResultJson_DoesNotRenderResultPanel()
    {
        IRenderedComponent<EngineConsoleComponent> component = RenderComponent<EngineConsoleComponent>(p => p.Add(c => c.Title, "T"));

        component.FindAll(".ps-engine-console__result").Should().BeEmpty();
    }

    [Fact]
    public void Render_ResultJsonSupplied_RendersResultPanel()
    {
        IRenderedComponent<EngineConsoleComponent> component = RenderComponent<EngineConsoleComponent>(p => p
            .Add(c => c.Title, "T")
            .Add(c => c.ResultJson, "{\"ok\":true}"));

        component.Find(".ps-engine-console__result pre").TextContent.Should().Be("{\"ok\":true}");
    }
}
