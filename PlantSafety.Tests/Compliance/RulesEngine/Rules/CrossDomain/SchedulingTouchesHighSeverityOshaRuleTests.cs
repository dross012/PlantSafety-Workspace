// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.RulesEngine.Rules.CrossDomain
//  Layer:          Tests (Unit)
//  File:           SchedulingTouchesHighSeverityOshaRuleTests.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SchedulingTouchesHighSeverityOshaRule. Pure logic, no I/O.
//                  Mirrors PermitRequiresTrainingRuleTests.cs's Arrange/Act/Assert style.
// ============================================================================
using PlantSafety.Application.Compliance.RulesEngine.Rules;
using PlantSafety.Application.Compliance.RulesEngine.Rules.CrossDomain;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Tests.Compliance.RulesEngine.Rules.CrossDomain;

public sealed class SchedulingTouchesHighSeverityOshaRuleTests : UnitTestBase
{
    private static readonly SchedulingTouchesHighSeverityOshaRule Rule = new();

    private static ComplianceRuleContext ContextFor(string? oshaModule, bool? hasHighSeverityRequirements) =>
        new(
            TestTenantId.Value,
            null,
            "PMScheduling",
            Guid.NewGuid(),
            FixedTimestamp.Value,
            new Dictionary<string, string?>
            {
                ["Osha.Module"] = oshaModule,
                ["Osha.HasHighSeverityRequirements"] = hasHighSeverityRequirements?.ToString(),
            });

    [Fact]
    public void Evaluate_ModuleHasHighSeverityRequirements_FlagsAdvisory()
    {
        var outcome = Rule.Evaluate(ContextFor("MachineSafety", hasHighSeverityRequirements: true));

        outcome.Passed.Should().BeFalse();
        outcome.Violations.Should().ContainSingle(v =>
            v.ViolationCode == "Scheduling.TouchesHighSeverityOsha" &&
            v.Severity == ComplianceSeverity.Advisory &&
            v.Description.Contains("MachineSafety"));
    }

    [Fact]
    public void Evaluate_ModuleHasHighSeverityRequirements_NeverFailsTheOverallEvaluation()
    {
        // ComplianceOutcomePolicy only fails an evaluation on High/Critical violations, or 2+
        // Medium ones -- Advisory alone never does. This is the behavior the whole point of this
        // rule depends on: it must inform, never block, scheduling.
        var outcome = Rule.Evaluate(ContextFor("MachineSafety", hasHighSeverityRequirements: true));

        outcome.Violations.Should().OnlyContain(v => v.Severity == ComplianceSeverity.Advisory);
    }

    [Fact]
    public void Evaluate_ModuleHasNoHighSeverityRequirements_Passes()
    {
        var outcome = Rule.Evaluate(ContextFor("MachineSafety", hasHighSeverityRequirements: false));

        outcome.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ModuleFactMissing_Passes()
    {
        // Matches every other IComplianceRule's convention: absent facts never fail closed.
        var outcome = Rule.Evaluate(ContextFor(oshaModule: null, hasHighSeverityRequirements: true));

        outcome.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_HighSeverityFactMissing_DefaultsToFalseAndPasses()
    {
        var outcome = Rule.Evaluate(ContextFor("MachineSafety", hasHighSeverityRequirements: null));

        outcome.Passed.Should().BeTrue();
    }
}
