// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           CallerContextFactoryTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Caller;

public sealed class CallerContextFactoryTests
{
    private static readonly IReadOnlyDictionary<string, EnterpriseRole> RegisteredKeys = new Dictionary<string, EnterpriseRole>
    {
        ["editor-key"] = EnterpriseRole.Editor,
    };

    [Fact]
    public void Create_NoKeyProvided_FallsBackToSystemDefault()
    {
        CallerContextResolution resolution = CallerContextFactory.Create(null, RegisteredKeys, "corr-1");

        resolution.Caller.Should().Be(CallerContext.SystemDefault("corr-1"));
        resolution.Identity.KeyProvided.Should().BeFalse();
        resolution.Identity.KeyRecognized.Should().BeFalse();
    }

    [Fact]
    public void Create_UnrecognizedKey_FallsBackToSystemDefaultButFlagsKeyProvided()
    {
        CallerContextResolution resolution = CallerContextFactory.Create("bogus", RegisteredKeys, "corr-2");

        resolution.Caller.Should().Be(CallerContext.SystemDefault("corr-2"));
        resolution.Identity.KeyProvided.Should().BeTrue();
        resolution.Identity.KeyRecognized.Should().BeFalse();
    }

    [Fact]
    public void Create_RecognizedKey_ResolvesMappedRoleWithNotApplicableMfa()
    {
        CallerContextResolution resolution = CallerContextFactory.Create("editor-key", RegisteredKeys, "corr-3");

        resolution.Caller.Role.Should().Be(EnterpriseRole.Editor);
        resolution.Caller.MfaStatus.Should().Be(MfaStatus.NotApplicable);
        resolution.Caller.CorrelationId.Should().Be("corr-3");
        resolution.Identity.KeyProvided.Should().BeTrue();
        resolution.Identity.KeyRecognized.Should().BeTrue();
    }

    [Fact]
    public void Create_AlwaysPropagatesTheSuppliedCorrelationId()
    {
        CallerContextFactory.Create(null, RegisteredKeys, "corr-x").Caller.CorrelationId.Should().Be("corr-x");
        CallerContextFactory.Create("editor-key", RegisteredKeys, "corr-y").Caller.CorrelationId.Should().Be("corr-y");
    }
}
