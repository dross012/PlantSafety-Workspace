// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           CallerContextFactoryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Identity;

public sealed class CallerContextFactoryTests
{
    private static readonly IReadOnlyDictionary<string, EnterpriseRole> RegisteredKeys = new Dictionary<string, EnterpriseRole>
    {
        ["key-viewer"] = EnterpriseRole.Viewer,
    };

    [Fact]
    public void Create_NoHeaderValue_FallsBackToSystemDefaultWithNoKeyProvided()
    {
        CallerContextResolution resolution = CallerContextFactory.Create(null, RegisteredKeys, "corr-1");

        resolution.Outcome.Should().Be(CallerContextResolutionOutcome.NoKeyProvided);
        resolution.Caller.Role.Should().Be(EnterpriseRole.PlatformAdmin);
    }

    [Fact]
    public void Create_RecognizedKey_ResolvesToRegisteredRole()
    {
        CallerContextResolution resolution = CallerContextFactory.Create("key-viewer", RegisteredKeys, "corr-2");

        resolution.Outcome.Should().Be(CallerContextResolutionOutcome.Recognized);
        resolution.Caller.Role.Should().Be(EnterpriseRole.Viewer);
    }

    [Fact]
    public void Create_UnrecognizedKey_FallsBackToSystemDefault()
    {
        CallerContextResolution resolution = CallerContextFactory.Create("nonexistent-key", RegisteredKeys, "corr-3");

        resolution.Outcome.Should().Be(CallerContextResolutionOutcome.Unrecognized);
        resolution.Caller.Role.Should().Be(EnterpriseRole.PlatformAdmin);
    }

    [Fact]
    public void Create_PropagatesCorrelationId()
    {
        CallerContextFactory.Create(null, RegisteredKeys, "corr-4").Caller.CorrelationId.Should().Be("corr-4");
    }
}
