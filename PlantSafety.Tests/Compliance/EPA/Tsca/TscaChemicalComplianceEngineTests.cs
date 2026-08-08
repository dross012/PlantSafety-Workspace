// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.EPA.Tsca
//  Layer:          Tests (Unit)
//  File:           TscaChemicalComplianceEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the TscaChemicalComplianceEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.Tsca.Services;

namespace PlantSafety.Tests.Compliance.EPA.Tsca;

/// <summary>Unit tests for <see cref="TscaChemicalComplianceEngine"/>.</summary>
public sealed class TscaChemicalComplianceEngineTests : UnitTestBase
{
    private readonly TscaChemicalComplianceEngine _engine = new();

    [Fact]
    public void EvaluateCompliance_NewChemicalPmnFiledTooLate_ShouldReportCriticalViolation()
    {
        var evaluatedAtUtc = FixedTimestamp;
        var plannedDate = UtcTimestamp.From(FixedUtcNow.AddDays(30));
        var noticeSubmitted = UtcTimestamp.From(FixedUtcNow.AddDays(10)); // only 20 days before planned date, < 90

        var input = new TscaComplianceInput(
            Chemicals:
            [
                new TscaChemicalRecord(
                    ChemicalName: "Novel Polymer X",
                    CasNumber: "999-99-9",
                    IsOnTscaInventory: false,
                    IsNewChemicalNotYetManufactured: true,
                    PlannedManufactureOrImportDateUtc: plannedDate,
                    IsSubjectToSnur: false,
                    HasSubmittedRequiredNotice: true,
                    NoticeSubmittedAtUtc: noticeSubmitted,
                    IsRestrictedUnderSection5f: false),
            ],
            EvaluatedAtUtc: evaluatedAtUtc);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Citation.Contains("40 CFR 720"));
    }

    [Fact]
    public void EvaluateCompliance_ChemicalAlreadyOnInventory_ShouldReportNoViolations()
    {
        var input = new TscaComplianceInput(
            Chemicals:
            [
                new TscaChemicalRecord(
                    ChemicalName: "Common Solvent",
                    CasNumber: "111-11-1",
                    IsOnTscaInventory: true,
                    IsNewChemicalNotYetManufactured: false,
                    PlannedManufactureOrImportDateUtc: null,
                    IsSubjectToSnur: false,
                    HasSubmittedRequiredNotice: false,
                    NoticeSubmittedAtUtc: null,
                    IsRestrictedUnderSection5f: false),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_SnurTriggeredWithoutNotice_ShouldReportCriticalViolation()
    {
        var input = new TscaComplianceInput(
            Chemicals:
            [
                new TscaChemicalRecord(
                    ChemicalName: "Restricted Surfactant",
                    CasNumber: "222-22-2",
                    IsOnTscaInventory: true,
                    IsNewChemicalNotYetManufactured: false,
                    PlannedManufactureOrImportDateUtc: null,
                    IsSubjectToSnur: true,
                    HasSubmittedRequiredNotice: false,
                    NoticeSubmittedAtUtc: null,
                    IsRestrictedUnderSection5f: false),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Citation.Contains("SNUR"));
    }

    [Fact]
    public void EvaluateCompliance_Section5fRestrictedChemical_ShouldReportHighWarningNotViolation()
    {
        var input = new TscaComplianceInput(
            Chemicals:
            [
                new TscaChemicalRecord(
                    ChemicalName: "Legacy Flame Retardant",
                    CasNumber: "333-33-3",
                    IsOnTscaInventory: true,
                    IsNewChemicalNotYetManufactured: false,
                    PlannedManufactureOrImportDateUtc: null,
                    IsSubjectToSnur: false,
                    HasSubmittedRequiredNotice: false,
                    NoticeSubmittedAtUtc: null,
                    IsRestrictedUnderSection5f: true),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().ContainSingle(w => w.Severity == "High" && w.Citation.Contains("5(f)"));
    }

    [Fact]
    public void EvaluateCompliance_EmptyChemicalName_ShouldFail()
    {
        var input = new TscaComplianceInput(
            Chemicals:
            [
                new TscaChemicalRecord(
                    ChemicalName: "",
                    CasNumber: null,
                    IsOnTscaInventory: true,
                    IsNewChemicalNotYetManufactured: false,
                    PlannedManufactureOrImportDateUtc: null,
                    IsSubjectToSnur: false,
                    HasSubmittedRequiredNotice: false,
                    NoticeSubmittedAtUtc: null,
                    IsRestrictedUnderSection5f: false),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
    }
}
