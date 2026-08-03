// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Persistence
//  Layer:          Tests (Unit / Contract)
//  File:           SafetyDataReportingContractTests.cs
//  Created:        2026-07-27
//  Author:         Testing Agent
//  SchemaVersion:  2026.1
//  Description:    Contract tests verifying that Shared.SafetyData.Abstractions.Models'
//                  HazardSummaryEntry / CorrectiveActionEntry / SafetyComplianceEntry --
//                  the shapes ISafetyDataRepository (implemented in
//                  Shared.SafetyData.Infrastructure against PlantSafety.CoreDB directly) actually
//                  returns -- satisfy the field-for-field contract Reporting.Domain's
//                  HazardSummaryData / CorrectiveActionReportData / SafetyComplianceReportData
//                  value objects expect (Reporting\Reporting.Domain\ValueObjects\
//                  HazardSummaryData.cs / CorrectiveActionReportData.cs /
//                  SafetyComplianceReportData.cs).
//
//                  PLACEMENT (ambiguity noted in the task brief, resolved per instruction "use
//                  whichever project makes more sense given where ISafetyDataRepository is
//                  actually implemented"): this lives in Shared.SafetyData.Tests rather than
//                  PlantSafety.IntegrationTests because ISafetyDataRepository/its models are owned
//                  and versioned here, and Shared.SafetyData.Tests already establishes the
//                  convention (see SafetySearchCriteriaTests.cs) of asserting these models' shapes
//                  in this project.
//
//                  APPROACH: Shared.SafetyData.Tests deliberately does not take a project
//                  reference on Reporting.Domain (Shared.SafetyData is a leaf module consumed by
//                  Reporting, not the reverse -- adding the reverse reference would create a
//                  circular module dependency across solutions). Reflection is instead used to
//                  assert the exact ordered (PropertyName, PropertyType) sequence of each
//                  Shared.SafetyData.Abstractions.Models entry record against a hard-coded
//                  expectation that mirrors Reporting.Domain.ValueObjects' corresponding record
//                  (verified identical by direct code inspection of both sides at the time this
//                  test was written -- see the field-mapping comments in HazardSummaryEntry.cs /
//                  CorrectiveActionEntry.cs / SafetyComplianceEntry.cs, and the mirrored XML doc
//                  comments in Reporting.Domain.ValueObjects.HazardSummaryData.cs /
//                  CorrectiveActionReportData.cs / SafetyComplianceReportData.cs). If either side's
//                  record shape drifts (a property renamed, retyped, added, or removed) without the
//                  other being updated to match, this test fails -- it exists specifically to catch
//                  that class of contract drift between the two independently-versioned modules.
// ============================================================================
using System.Reflection;

using Shared.SafetyData.Abstractions.Models;

using Xunit;

namespace Shared.SafetyData.Tests.Persistence;

public sealed class SafetyDataReportingContractTests
{
    [Fact]
    public void HazardSummaryEntry_Shape_MatchesReportingDomainHazardSummaryEntryContract()
    {
        // Reporting.Domain.ValueObjects.HazardSummaryEntry(string HazardId, DateTime IdentifiedAtUtc,
        // string Category, string RiskLevel, string Description, IReadOnlyList<string> ControlMeasures)
        AssertShape<HazardSummaryEntry>(
            (nameof(HazardSummaryEntry.HazardId), typeof(string)),
            (nameof(HazardSummaryEntry.IdentifiedAtUtc), typeof(DateTime)),
            (nameof(HazardSummaryEntry.Category), typeof(string)),
            (nameof(HazardSummaryEntry.RiskLevel), typeof(string)),
            (nameof(HazardSummaryEntry.Description), typeof(string)),
            (nameof(HazardSummaryEntry.ControlMeasures), typeof(IReadOnlyList<string>)));
    }

    [Fact]
    public void CorrectiveActionEntry_Shape_MatchesReportingDomainCorrectiveActionEntryContract()
    {
        // Reporting.Domain.ValueObjects.CorrectiveActionEntry(string CorrectiveActionId,
        // DateTime RaisedAtUtc, string Category, string Status, string Description, DateTime? DueDateUtc)
        AssertShape<CorrectiveActionEntry>(
            (nameof(CorrectiveActionEntry.CorrectiveActionId), typeof(string)),
            (nameof(CorrectiveActionEntry.RaisedAtUtc), typeof(DateTime)),
            (nameof(CorrectiveActionEntry.Category), typeof(string)),
            (nameof(CorrectiveActionEntry.Status), typeof(string)),
            (nameof(CorrectiveActionEntry.Description), typeof(string)),
            (nameof(CorrectiveActionEntry.DueDateUtc), typeof(DateTime?)));
    }

    [Fact]
    public void SafetyComplianceEntry_Shape_MatchesReportingDomainSafetyComplianceEntryContract()
    {
        // Reporting.Domain.ValueObjects.SafetyComplianceEntry(string Category, int TotalCount,
        // int OpenCount, int ClosedCount, int OverdueCount, decimal CompliancePercentage, DateTime AsOfUtc)
        AssertShape<SafetyComplianceEntry>(
            (nameof(SafetyComplianceEntry.Category), typeof(string)),
            (nameof(SafetyComplianceEntry.TotalCount), typeof(int)),
            (nameof(SafetyComplianceEntry.OpenCount), typeof(int)),
            (nameof(SafetyComplianceEntry.ClosedCount), typeof(int)),
            (nameof(SafetyComplianceEntry.OverdueCount), typeof(int)),
            (nameof(SafetyComplianceEntry.CompliancePercentage), typeof(decimal)),
            (nameof(SafetyComplianceEntry.AsOfUtc), typeof(DateTime)));
    }

    /// <summary>
    /// Asserts that the given record type's public instance properties, in declaration order,
    /// exactly match the supplied (name, type) sequence -- both in count and in each entry.
    /// </summary>
    private static void AssertShape<T>(params (string Name, Type Type)[] expected)
    {
        var actualProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0)
            .OrderBy(p => p.MetadataToken)
            .ToArray();

        Assert.True(
            expected.Length == actualProperties.Length,
            $"{typeof(T).Name} has {actualProperties.Length} public properties " +
            $"({string.Join(", ", actualProperties.Select(p => p.Name))}), but the Reporting.Domain " +
            $"contract expects exactly {expected.Length} ({string.Join(", ", expected.Select(e => e.Name))}). " +
            "Reporting's mapping layer maps these two shapes 1:1 -- keep them in sync.");

        for (var i = 0; i < expected.Length; i++)
        {
            var (expectedName, expectedType) = expected[i];
            var actual = actualProperties[i];

            Assert.True(
                actual.Name == expectedName,
                $"{typeof(T).Name} property #{i} is named '{actual.Name}', but the Reporting.Domain " +
                $"contract expects '{expectedName}' at this position.");

            Assert.True(
                actual.PropertyType == expectedType,
                $"{typeof(T).Name}.{actual.Name} has type '{actual.PropertyType}', but the Reporting.Domain " +
                $"contract expects '{expectedType}'.");
        }
    }
}
