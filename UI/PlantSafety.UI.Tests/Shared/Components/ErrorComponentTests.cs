// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           ErrorComponentTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for ErrorComponent: renders the backend
//                  error message verbatim (never rewritten), conditional
//                  Retry button, and the role="alert" accessibility contract.
// ============================================================================
using Bunit;

using AwesomeAssertions;

using PlantSafety.UI.Shared.Components;

using Xunit;

namespace PlantSafety.UI.Tests.Shared.Components;

public sealed class ErrorComponentTests : TestContext
{
    [Fact]
    public void Render_ShowsErrorMessageVerbatim()
    {
        IRenderedComponent<ErrorComponent> component = RenderComponent<ErrorComponent>(p => p.Add(c => c.Error, "Response status code does not indicate success: 500."));

        component.Find(".ps-error__message").TextContent.Should().Be("Response status code does not indicate success: 500.");
    }

    [Fact]
    public void Render_HasAlertRole()
    {
        IRenderedComponent<ErrorComponent> component = RenderComponent<ErrorComponent>(p => p.Add(c => c.Error, "Boom"));

        component.Find(".ps-error").GetAttribute("role").Should().Be("alert");
    }

    [Fact]
    public void Render_NoRetryDelegate_DoesNotRenderRetryButton()
    {
        IRenderedComponent<ErrorComponent> component = RenderComponent<ErrorComponent>(p => p.Add(c => c.Error, "Boom"));

        component.FindAll(".ps-error__retry").Should().BeEmpty();
    }

    [Fact]
    public void Render_RetryDelegateSupplied_ClickingRetryInvokesIt()
    {
        var retried = false;
        IRenderedComponent<ErrorComponent> component = RenderComponent<ErrorComponent>(p => p
            .Add(c => c.Error, "Boom")
            .Add(c => c.OnRetry, () => { retried = true; }));

        component.Find(".ps-error__retry").Click();

        retried.Should().BeTrue();
    }
}
