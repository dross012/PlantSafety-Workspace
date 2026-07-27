// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.RiskManagement
//  Layer:          Tests (Unit)
//  File:           ControlMeasureTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the ControlMeasure child entity: construction defaults and
//                  justification-trimming behavior. The RequiresJustification invariant itself is
//                  enforced by the parent Risk aggregate (see RiskAggregateTests.ApplyControlMeasure_*).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Entities;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Tests.Domain.RiskManagement;

/// <summary>Unit tests for <see cref="ControlMeasure"/>.</summary>
public sealed class ControlMeasureTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var measure = new ControlMeasure(RiskId.New(), ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp);
        measure.ControlMeasureId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetHierarchyLevel()
    {
        var measure = new ControlMeasure(RiskId.New(), ControlHierarchyLevel.Ppe, "Issue gloves", "Justified", TestUserId, FixedTimestamp);
        measure.HierarchyLevel.Should().Be(ControlHierarchyLevel.Ppe);
    }

    [Fact]
    public void Create_ShouldTrimDescription()
    {
        var measure = new ControlMeasure(RiskId.New(), ControlHierarchyLevel.Elimination, "  Remove hazard  ", null, TestUserId, FixedTimestamp);
        measure.Description.Should().Be("Remove hazard");
    }

    [Fact]
    public void Create_WithNullJustification_ShouldStoreNull()
    {
        var measure = new ControlMeasure(RiskId.New(), ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp);
        measure.Justification.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceJustification_ShouldStoreNull()
    {
        var measure = new ControlMeasure(RiskId.New(), ControlHierarchyLevel.Ppe, "Issue gloves", "   ", TestUserId, FixedTimestamp);
        measure.Justification.Should().BeNull();
    }

    [Fact]
    public void Create_WithJustification_ShouldTrimAndStore()
    {
        var measure = new ControlMeasure(RiskId.New(), ControlHierarchyLevel.Ppe, "Issue gloves", "  Elimination not feasible  ", TestUserId, FixedTimestamp);
        measure.Justification.Should().Be("Elimination not feasible");
    }

    [Fact]
    public void Create_ShouldSetResponsiblePersonAndImplementedAt()
    {
        var measure = new ControlMeasure(RiskId.New(), ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp);
        measure.ResponsiblePersonId.Should().Be(TestUserId);
        measure.ImplementedAtUtc.Should().Be(FixedTimestamp);
    }
}
