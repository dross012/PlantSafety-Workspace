// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           ApiKeyValidatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Identity;

public sealed class ApiKeyValidatorTests
{
    private static readonly IReadOnlyDictionary<string, EnterpriseRole> RegisteredKeys = new Dictionary<string, EnterpriseRole>
    {
        ["editor-key"] = EnterpriseRole.Editor,
        ["admin-key"] = EnterpriseRole.PlatformAdmin,
    };

    [Fact]
    public void Resolve_NullKey_ReturnsNull()
    {
        ApiKeyValidator.Resolve(null, RegisteredKeys).Should().BeNull();
    }

    [Fact]
    public void Resolve_EmptyOrWhitespaceKey_ReturnsNull()
    {
        ApiKeyValidator.Resolve("", RegisteredKeys).Should().BeNull();
        ApiKeyValidator.Resolve("   ", RegisteredKeys).Should().BeNull();
    }

    [Fact]
    public void Resolve_UnrecognizedKey_ReturnsNull()
    {
        ApiKeyValidator.Resolve("not-registered", RegisteredKeys).Should().BeNull();
    }

    [Fact]
    public void Resolve_RegisteredKey_ReturnsMappedRole()
    {
        ApiKeyValidator.Resolve("editor-key", RegisteredKeys).Should().Be(EnterpriseRole.Editor);
        ApiKeyValidator.Resolve("admin-key", RegisteredKeys).Should().Be(EnterpriseRole.PlatformAdmin);
    }

    [Fact]
    public void Resolve_NullRegisteredKeys_Throws()
    {
        Action act = () => ApiKeyValidator.Resolve("editor-key", null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
