// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphMetadataExtractorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.SafetyGraph.Abstractions.Construction;
using Shared.SafetyGraph.Abstractions.Graph;

using Xunit;

namespace Shared.SafetyGraph.Tests.Construction;

public sealed class SafetyGraphMetadataExtractorTests
{
    [Fact]
    public void Extract_ProducesExpectedMetadata()
    {
        var issuedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        SafetyGraphMetadata metadata = SafetyGraphMetadataExtractor.Extract("acme-plant-1", "corr-1", issuedAtUtc);

        metadata.SchemaVersion.Should().Be(SafetyGraphMetadata.CurrentSchemaVersion);
        metadata.TenantId.Should().Be("acme-plant-1");
        metadata.CorrelationId.Should().Be("corr-1");
        metadata.IssuedAtUtc.Should().Be(issuedAtUtc);
    }

    [Fact]
    public void Extract_NullTenantId_Throws()
    {
        Action act = () => SafetyGraphMetadataExtractor.Extract(null!, "corr-1", DateTime.UtcNow);

        act.Should().Throw<ArgumentNullException>();
    }
}
