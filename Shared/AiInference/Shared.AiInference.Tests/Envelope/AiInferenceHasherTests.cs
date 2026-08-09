// ============================================================================
//  Project:        Shared.AiInference.Tests
//  File:           AiInferenceHasherTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.AiInference.Abstractions.Envelope;

using Xunit;

namespace Shared.AiInference.Tests.Envelope;

public sealed class AiInferenceHasherTests
{
    [Fact]
    public void Hash_SameInput_ProducesTheSameHash()
    {
        AiInferenceHasher.Hash("{\"hazardType\":\"Fire\"}").Should().Be(AiInferenceHasher.Hash("{\"hazardType\":\"Fire\"}"));
    }

    [Fact]
    public void Hash_DifferentInput_ProducesDifferentHashes()
    {
        AiInferenceHasher.Hash("{\"hazardType\":\"Fire\"}").Should().NotBe(AiInferenceHasher.Hash("{\"hazardType\":\"ChemicalSpill\"}"));
    }

    [Fact]
    public void Hash_ReturnsLowercaseHex()
    {
        string hash = AiInferenceHasher.Hash("test");

        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }
}
