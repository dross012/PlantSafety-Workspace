// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Tsca
//  Layer:          Domain
//  File:           TscaChemicalComplianceEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates TSCA
//                  (15 U.S.C. §2601, 40 CFR Part 700+) chemical-substance compliance for a plant's
//                  chemical inventory. This codebase does not yet have a persisted chemical-inventory
//                  Domain aggregate. Until one exists, callers must supply this data directly (e.g.
//                  from an SDS import process or manual entry) rather than loading it from a
//                  repository (same "stateless calculation service" pattern as
//                  Compliance.PHMSA.Segregation.Services.SegregationEngine).
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - The TSCA Chemical Substances Inventory (maintained by EPA) lists chemicals
//                      approved for US manufacture/import by CAS number; manufacturing, importing,
//                      or processing a chemical not on the Inventory without following the New
//                      Chemicals process is unlawful.
//                    - TSCA Section 5, 40 CFR 720: a Premanufacture Notice (PMN) must be submitted
//                      at least 90 days before manufacturing/importing a new chemical substance not
//                      on the Inventory.
//                    - TSCA Section 5(a), 40 CFR 721: a Significant New Use Rule (SNUR) requires a
//                      Significant New Use Notice (SNUN) at least 90 days before a significant new
//                      use of a listed chemical begins.
//                    - TSCA Section 5(f): EPA may restrict or prohibit manufacture, processing, or
//                      distribution of a chemical found to present unreasonable risk.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.Shared;
using PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.EPA.Tsca.Services;

/// <summary>Input to <see cref="TscaChemicalComplianceEngine.EvaluateCompliance"/>.</summary>
public sealed record TscaComplianceInput(
    IReadOnlyList<TscaChemicalRecord> Chemicals,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Caller-supplied snapshot of a single chemical substance's TSCA regulatory posture. This codebase
/// does not yet have a persisted Domain aggregate for chemical inventory. Until one exists, callers
/// must supply this data directly (e.g. from an SDS import process or manual entry) rather than
/// loading it from a repository.
/// </summary>
public sealed record TscaChemicalRecord(
    string ChemicalName,
    string? CasNumber,
    bool IsOnTscaInventory,
    bool IsNewChemicalNotYetManufactured,
    UtcTimestamp? PlannedManufactureOrImportDateUtc,
    bool IsSubjectToSnur,
    bool HasSubmittedRequiredNotice,
    UtcTimestamp? NoticeSubmittedAtUtc,
    bool IsRestrictedUnderSection5f);

/// <summary>
/// Evaluates TSCA (15 U.S.C. §2601, 40 CFR Part 700+) chemical-substance compliance for a plant's
/// chemical inventory. Pure/stateless: this codebase does not yet have a persisted
/// chemical-inventory Domain aggregate, so callers must supply chemical records directly (e.g. from
/// an SDS import) until one exists.
/// </summary>
public sealed class TscaChemicalComplianceEngine : IDomainService
{
    private const string ProgramCode = "TSCA";

    /// <summary>Minimum number of days a PMN/SNUN must precede manufacture/import or significant new use (40 CFR 720, 40 CFR 721).</summary>
    private const int MinimumNoticeLeadTimeDays = 90;

    /// <summary>Evaluates TSCA compliance for the supplied chemical inventory snapshot.</summary>
    public Result<EpaComplianceReport> EvaluateCompliance(TscaComplianceInput input)
    {
        if (input is null)
            return Result<EpaComplianceReport>.Failure("Input is required.");
        if (input.Chemicals is null)
            return Result<EpaComplianceReport>.Failure("Chemicals list is required.");

        foreach (var chemical in input.Chemicals)
        {
            if (chemical is null || string.IsNullOrWhiteSpace(chemical.ChemicalName))
                return Result<EpaComplianceReport>.Failure("Each chemical record must have a non-empty ChemicalName.");
        }

        var violations = new List<EpaComplianceFinding>();
        var warnings = new List<EpaComplianceFinding>();
        var requiredActions = new List<string>();

        foreach (var chemical in input.Chemicals)
        {
            EvaluateNewChemicalPmn(chemical, violations, requiredActions);
            EvaluateSnur(chemical, input.EvaluatedAtUtc, violations, requiredActions);
            EvaluateSection5fRestriction(chemical, warnings, requiredActions);
        }

        var report = new EpaComplianceReport(
            ProgramCode: ProgramCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<EpaComplianceReport>.Success(report);
    }

    /// <summary>
    /// TSCA Section 5, 40 CFR 720: any chemical not on the Inventory that is planned for
    /// manufacture/import must have a PMN filed at least 90 days before the planned date.
    /// </summary>
    private static void EvaluateNewChemicalPmn(
        TscaChemicalRecord chemical,
        List<EpaComplianceFinding> violations,
        List<string> requiredActions)
    {
        if (chemical.IsOnTscaInventory || !chemical.IsNewChemicalNotYetManufactured)
            return;

        var plannedDate = chemical.PlannedManufactureOrImportDateUtc;

        var noticeIsMissing = !chemical.HasSubmittedRequiredNotice;
        var noticeIsLate = chemical.HasSubmittedRequiredNotice
            && chemical.NoticeSubmittedAtUtc is not null
            && plannedDate is not null
            && (plannedDate.Value.Value - chemical.NoticeSubmittedAtUtc.Value.Value).TotalDays < MinimumNoticeLeadTimeDays;

        if (!noticeIsMissing && !noticeIsLate)
            return;

        var description = noticeIsMissing
            ? $"Chemical '{chemical.ChemicalName}' ({chemical.CasNumber ?? "no CAS number"}) is not on the TSCA Inventory and is planned for manufacture/import, but no Premanufacture Notice (PMN) has been submitted."
            : $"Chemical '{chemical.ChemicalName}' ({chemical.CasNumber ?? "no CAS number"}) had its Premanufacture Notice (PMN) submitted less than {MinimumNoticeLeadTimeDays} days before the planned manufacture/import date.";

        violations.Add(new EpaComplianceFinding(
            Citation: "TSCA Section 5, 40 CFR 720",
            Description: description,
            Severity: "Critical",
            RelatedEntityId: chemical.CasNumber));

        requiredActions.Add(
            $"Submit a Premanufacture Notice (PMN) for '{chemical.ChemicalName}' at least {MinimumNoticeLeadTimeDays} days before manufacture/import, per TSCA Section 5, 40 CFR 720.");
    }

    /// <summary>
    /// TSCA Section 5(a), 40 CFR 721 (SNUR): a chemical subject to a Significant New Use Rule must
    /// have a Significant New Use Notice (SNUN) filed at least 90 days before the new use begins.
    /// </summary>
    private static void EvaluateSnur(
        TscaChemicalRecord chemical,
        UtcTimestamp evaluatedAtUtc,
        List<EpaComplianceFinding> violations,
        List<string> requiredActions)
    {
        if (!chemical.IsSubjectToSnur)
            return;

        var noticeIsMissing = !chemical.HasSubmittedRequiredNotice;
        var noticeIsLate = chemical.HasSubmittedRequiredNotice
            && chemical.NoticeSubmittedAtUtc is not null
            && (evaluatedAtUtc.Value - chemical.NoticeSubmittedAtUtc.Value.Value).TotalDays < MinimumNoticeLeadTimeDays
            && chemical.NoticeSubmittedAtUtc.Value.Value <= evaluatedAtUtc.Value;

        if (!noticeIsMissing && !noticeIsLate)
            return;

        var description = noticeIsMissing
            ? $"Chemical '{chemical.ChemicalName}' ({chemical.CasNumber ?? "no CAS number"}) is subject to a Significant New Use Rule (SNUR), but no Significant New Use Notice (SNUN) has been submitted."
            : $"Chemical '{chemical.ChemicalName}' ({chemical.CasNumber ?? "no CAS number"}) had its Significant New Use Notice (SNUN) submitted less than {MinimumNoticeLeadTimeDays} days before the significant new use.";

        violations.Add(new EpaComplianceFinding(
            Citation: "TSCA Section 5(a), 40 CFR 721 (SNUR)",
            Description: description,
            Severity: "Critical",
            RelatedEntityId: chemical.CasNumber));

        requiredActions.Add(
            $"Submit a Significant New Use Notice (SNUN) for '{chemical.ChemicalName}' at least {MinimumNoticeLeadTimeDays} days before the significant new use begins, per TSCA Section 5(a), 40 CFR 721.");
    }

    /// <summary>
    /// TSCA Section 5(f): chemicals subject to an EPA-imposed restriction are not automatically
    /// non-compliant -- the restriction may be actively followed -- but usage should be verified
    /// against the specific restriction terms, so this is flagged as a warning, not a violation.
    /// </summary>
    private static void EvaluateSection5fRestriction(
        TscaChemicalRecord chemical,
        List<EpaComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (!chemical.IsRestrictedUnderSection5f)
            return;

        warnings.Add(new EpaComplianceFinding(
            Citation: "TSCA Section 5(f)",
            Description: $"Chemical '{chemical.ChemicalName}' ({chemical.CasNumber ?? "no CAS number"}) is subject to an EPA-imposed restriction/prohibition under TSCA Section 5(f). Verify current usage against the specific restriction terms.",
            Severity: "High",
            RelatedEntityId: chemical.CasNumber));

        requiredActions.Add(
            $"Verify that current use of '{chemical.ChemicalName}' complies with the specific EPA restriction terms imposed under TSCA Section 5(f).");
    }
}
