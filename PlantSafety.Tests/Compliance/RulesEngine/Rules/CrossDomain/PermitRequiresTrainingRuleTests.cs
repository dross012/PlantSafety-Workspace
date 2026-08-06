// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.RulesEngine.Rules.CrossDomain
//  Layer:          Tests (Unit)
//  File:           PermitRequiresTrainingRuleTests.cs
//  Created:        2026-08-05
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PermitRequiresTrainingRule -- the first cross-domain
//                  IComplianceRule in the codebase. Pure logic, no I/O, no substitutes needed
//                  (Evaluate takes an already-built ComplianceRuleContext). Mirrors the shape of
//                  the existing single-domain rule tests' Arrange/Act/Assert style.
// ============================================================================
using PlantSafety.Application.Compliance.RulesEngine.Rules;
using PlantSafety.Application.Compliance.RulesEngine.Rules.CrossDomain;

namespace PlantSafety.Tests.Compliance.RulesEngine.Rules.CrossDomain;

public sealed class PermitRequiresTrainingRuleTests : UnitTestBase
{
    private static readonly PermitRequiresTrainingRule Rule = new();

    private static ComplianceRuleContext ContextFor(string? permitStatus, bool? hasTrainingRecord) =>
        new(
            TestTenantId.Value,
            null,
            "Permit",
            Guid.NewGuid(),
            FixedTimestamp.Value,
            new Dictionary<string, string?>
            {
                ["Permit.Status"] = permitStatus,
                ["Worker.HasTrainingRecordOnFile"] = hasTrainingRecord?.ToString(),
            });

    [Theory]
    [InlineData("Active")]
    [InlineData("Open")]
    [InlineData("InProgress")]
    [InlineData("Approved")]
    public void Evaluate_OpenPermitWorkerHasNoTrainingRecord_Fails(string openStatus)
    {
        var outcome = Rule.Evaluate(ContextFor(openStatus, hasTrainingRecord: false));

        outcome.Passed.Should().BeFalse();
        outcome.Violations.Should().ContainSingle(v => v.ViolationCode == "Permit.RequesterHasNoTrainingRecord");
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Open")]
    [InlineData("InProgress")]
    [InlineData("Approved")]
    public void Evaluate_OpenPermitWorkerHasTrainingRecord_Passes(string openStatus)
    {
        var outcome = Rule.Evaluate(ContextFor(openStatus, hasTrainingRecord: true));

        outcome.Passed.Should().BeTrue();
    }

    [Theory]
    [InlineData("Created")]
    [InlineData("Completed")]
    [InlineData("Closed")]
    [InlineData("Rejected")]
    [InlineData("Expired")]
    [InlineData("Archived")]
    public void Evaluate_NonOpenPermitWorkerHasNoTrainingRecord_Passes(string nonOpenStatus)
    {
        // Not-yet-active / already-terminal permits are out of scope for this rule -- only an
        // open/active permit represents authorized work in progress.
        var outcome = Rule.Evaluate(ContextFor(nonOpenStatus, hasTrainingRecord: false));

        outcome.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_PermitStatusFactMissing_Passes()
    {
        // Matches every other IComplianceRule's convention: absent facts never fail closed.
        var outcome = Rule.Evaluate(ContextFor(permitStatus: null, hasTrainingRecord: false));

        outcome.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_TrainingFactMissing_DefaultsToHasRecordAndPasses()
    {
        var outcome = Rule.Evaluate(ContextFor(permitStatus: "Active", hasTrainingRecord: null));

        outcome.Passed.Should().BeTrue();
    }
}
