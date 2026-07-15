// ============================================================================
//  Test IDs:  PLNT-DINT-ID-001..003
//  Manual:    project_plantsafety_patterns.md (Deterministic Identifier Rule
//             1.2); tests_rules_design_manual.md (DINT type).
//  Purpose:   Confirms Uuid7 produces strictly time-ordered, collision-free
//             identifiers -- the property every downstream data-integration
//             consumer (outbox replay, cross-module event ordering, curated
//             zone ingestion) relies on for replay-safe ordering without a
//             separate sequence column.
// ============================================================================
namespace PlantSafety.DataIntegrationTests.Identifiers;

public sealed class Uuid7DataIntegrationTests
{
    [Fact(DisplayName = "PLNT-DINT-ID-001: 50,000 rapidly-generated Uuid7 values are all unique (no collisions under burst generation)")]
    public void RapidGeneration_ShouldProduceNoCollisions()
    {
        const int count = 50_000;
        var ids = new HashSet<Guid>(count);

        for (var i = 0; i < count; i++)
            ids.Add(Uuid7.New());

        ids.Should().HaveCount(count, "every generated identifier must be unique even under rapid burst generation");
    }

    [Fact(DisplayName = "PLNT-DINT-ID-002: Sequentially generated Uuid7 values are monotonically non-decreasing (replay-safe ordering)")]
    public void SequentialGeneration_ShouldBeMonotonicallyNonDecreasing()
    {
        const int count = 10_000;
        var previous = Uuid7.New();
        var outOfOrder = 0;

        for (var i = 0; i < count; i++)
        {
            var current = Uuid7.New();
            if (CompareGuidBytes(current, previous) < 0)
                outOfOrder++;
            previous = current;
        }

        outOfOrder.Should().Be(0, "Uuid7 identifiers must be monotonically non-decreasing so consumers can rely on generation order for replay-safe data integration");
    }

    [Fact(DisplayName = "PLNT-DINT-ID-003: Uuid7 values are never the empty GUID (a data-integration key must always be a valid identity)")]
    public void GeneratedValues_ShouldNeverBeEmpty()
    {
        for (var i = 0; i < 1_000; i++)
            Uuid7.New().Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Compares two GUIDs by their raw <see cref="Guid.ToByteArray()"/> layout (bytes[0]
    /// first). Uuid7.New() writes the 48-bit timestamp directly into bytes[0..5] of the
    /// byte array passed to the <see cref="Guid(byte[])"/> constructor; ToByteArray() is a
    /// byte-for-byte round trip of that same array (it does NOT re-derive/reorder bytes the
    /// way Guid.ToString()/Guid.CompareTo() do for display purposes), so comparing the raw
    /// byte array is the correct way to observe Uuid7's intended chronological ordering.
    /// (Guid.CompareTo() must NOT be used here -- it reorders the first 8 bytes for its
    /// legacy COM/Windows GUID comparison semantics and would not reflect Uuid7's actual
    /// construction order.)
    /// </summary>
    private static int CompareGuidBytes(Guid a, Guid b)
    {
        var aBytes = a.ToByteArray();
        var bBytes = b.ToByteArray();

        for (var i = 0; i < aBytes.Length; i++)
        {
            var cmp = aBytes[i].CompareTo(bBytes[i]);
            if (cmp != 0) return cmp;
        }

        return 0;
    }
}
