// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           KpiMetricDefinition.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the canonical KPI catalog of the Safety Scorecards Module,
//                  taken verbatim from "Core Lagging Metrics" / "Core Leading Metrics" /
//                  "Core Cultural Metrics" and Appendix B -- Safety Metrics & Formulas of
//                  PlantSafety_Program_Structure_extract.txt. Metrics whose exact formula is not
//                  given in Appendix B (e.g. training completion, closure rates) are documented as
//                  ratio-based per the "System Features" bullets for that metric.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scorecards.Enums;

/// <summary>Smart Enum representing a single canonical KPI tracked by the Safety Scorecards Module.</summary>
public sealed class KpiMetricDefinition : IEquatable<KpiMetricDefinition>
{
    // ---- Lagging Indicators (Reactive Outcomes) ----

    /// <summary>TRIR = (Recordable Injuries x 200,000) / Total Hours Worked (Appendix B).</summary>
    public static readonly KpiMetricDefinition Trir = new("TRIR", "Total Recordable Incident Rate", IndicatorCategoryCodes.Lagging, "(Recordable Injuries x 200,000) / Total Hours Worked");

    /// <summary>DART = (DART Cases x 200,000) / Total Hours Worked (Appendix B).</summary>
    public static readonly KpiMetricDefinition Dart = new("DART", "Days Away, Restricted, or Transferred Rate", IndicatorCategoryCodes.Lagging, "(DART Cases x 200,000) / Total Hours Worked");

    /// <summary>Total number of days employees cannot work due to injury.</summary>
    public static readonly KpiMetricDefinition LostWorkdays = new("LostWorkdays", "Lost Workdays", IndicatorCategoryCodes.Lagging, "Sum of days away from work across all recordable injuries in the period");

    /// <summary>Number of OSHA-recordable injuries in a given period.</summary>
    public static readonly KpiMetricDefinition RecordableInjuryCount = new("RecordableInjuryCount", "Recordable Injury Count", IndicatorCategoryCodes.Lagging, "Count of OSHA-recordable injuries in the period");

    /// <summary>Frequency and cost of workers' compensation claims filed.</summary>
    public static readonly KpiMetricDefinition WorkersCompClaims = new("WorkersCompClaims", "Workers' Compensation Claims", IndicatorCategoryCodes.Lagging, "Count and total cost of claims filed in the period");

    // ---- Leading Indicators (Proactive Activities) ----

    /// <summary>Percentage of employees current on OSHA/PPE/LOTO/forklift/confined-space training.</summary>
    public static readonly KpiMetricDefinition TrainingCompletionRate = new("TrainingCompletionRate", "Training Completion Rate", IndicatorCategoryCodes.Leading, "(Employees Current on Required Training / Total Employees Required) x 100");

    /// <summary>Ratio of scheduled vs. completed safety walk-arounds, guarding inspections, PPE inspections, housekeeping audits.</summary>
    public static readonly KpiMetricDefinition AuditInspectionCompletionRate = new("AuditInspectionCompletionRate", "Audit & Inspection Completion Rate", IndicatorCategoryCodes.Leading, "(Completed Audits/Inspections / Scheduled Audits/Inspections) x 100");

    /// <summary>Number of open corrective actions, average days to close, percentage closed on time.</summary>
    public static readonly KpiMetricDefinition CorrectiveActionClosureRate = new("CorrectiveActionClosureRate", "Corrective Action Closure Rate", IndicatorCategoryCodes.Leading, "(Corrective Actions Closed On Time / Total Corrective Actions Due) x 100");

    /// <summary>PM Compliance % = (Completed PM Tasks / Scheduled PM Tasks) x 100 (Appendix B).</summary>
    public static readonly KpiMetricDefinition PreventiveMaintenanceCompliance = new("PreventiveMaintenanceCompliance", "Preventive Maintenance Compliance", IndicatorCategoryCodes.Leading, "(Completed PM Tasks / Scheduled PM Tasks) x 100");

    // ---- Cultural & Precursor Metrics ----

    /// <summary>Near-Miss Reporting Rate. High reporting = strong safety culture; low = underreporting or fear of retaliation.</summary>
    public static readonly KpiMetricDefinition NearMissReportingRate = new("NearMissReportingRate", "Near-Miss Reporting Rate", IndicatorCategoryCodes.Cultural, "(Near-Miss Reports Filed / Total Hours Worked) x Normalization Factor");

    /// <summary>Safety Suggestions & Stop-Work Actions -- measures employee empowerment and participation.</summary>
    public static readonly KpiMetricDefinition SafetySuggestionsStopWorkActions = new("SafetySuggestionsStopWorkActions", "Safety Suggestions & Stop-Work Actions", IndicatorCategoryCodes.Cultural, "Count of safety suggestions submitted plus stop-work actions invoked in the period");

    /// <summary>Behavioral Observations -- safe behaviors observed, at-risk behaviors documented, coaching conversations completed.</summary>
    public static readonly KpiMetricDefinition BehavioralObservationRate = new("BehavioralObservationRate", "Behavioral Observation Rate", IndicatorCategoryCodes.Cultural, "(Safe Behaviors Observed / Total Behaviors Observed) x 100");

    /// <summary>Safety Meeting Participation -- attendance and engagement levels.</summary>
    public static readonly KpiMetricDefinition SafetyMeetingParticipation = new("SafetyMeetingParticipation", "Safety Meeting Participation", IndicatorCategoryCodes.Cultural, "(Attendees / Expected Attendees) x 100");

    /// <summary>PPE Compliance % = (Compliant Observations / Total Observations) x 100 (Appendix B).</summary>
    public static readonly KpiMetricDefinition PpeComplianceRate = new("PpeComplianceRate", "PPE Compliance Rate", IndicatorCategoryCodes.Cultural, "(Compliant Observations / Total Observations) x 100");

    /// <summary>All defined KpiMetricDefinition instances.</summary>
    public static readonly IReadOnlyList<KpiMetricDefinition> All =
    [
        Trir, Dart, LostWorkdays, RecordableInjuryCount, WorkersCompClaims,
        TrainingCompletionRate, AuditInspectionCompletionRate, CorrectiveActionClosureRate, PreventiveMaintenanceCompliance,
        NearMissReportingRate, SafetySuggestionsStopWorkActions, BehavioralObservationRate, SafetyMeetingParticipation, PpeComplianceRate,
    ];

    /// <summary>The default KpiMetricDefinition instance.</summary>
    public static KpiMetricDefinition Default => Trir;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>The indicator category this KPI belongs to (Lagging/Leading/Cultural).</summary>
    public string IndicatorCategoryCode { get; }

    /// <summary>Human-readable formula description, taken from Appendix B where an exact formula is documented.</summary>
    public string FormulaDescription { get; }

    private KpiMetricDefinition(string code, string displayName, string indicatorCategoryCode, string formulaDescription)
    {
        Code = code;
        DisplayName = displayName;
        IndicatorCategoryCode = indicatorCategoryCode;
        FormulaDescription = formulaDescription;
    }

    /// <summary>Resolves a KpiMetricDefinition instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static KpiMetricDefinition From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown KpiMetricDefinition code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a KpiMetricDefinition instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out KpiMetricDefinition value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized KpiMetricDefinition code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <summary>
    /// Computes the metric value from a numerator/denominator pair using this metric's canonical
    /// formula. TRIR and DART apply the 200,000-hour OSHA normalization factor; percentage-style
    /// metrics multiply by 100; count-style metrics (LostWorkdays, RecordableInjuryCount,
    /// WorkersCompClaims, SafetySuggestionsStopWorkActions) return the numerator unchanged.
    /// </summary>
    public decimal ComputeValue(decimal numerator, decimal denominator)
    {
        if (Code is "TRIR" or "DART")
            return denominator == 0 ? 0m : Math.Round(numerator * 200_000m / denominator, 2);

        if (Code is "LostWorkdays" or "RecordableInjuryCount" or "WorkersCompClaims" or "SafetySuggestionsStopWorkActions")
            return numerator;

        return denominator == 0 ? 0m : Math.Round(numerator / denominator * 100m, 2);
    }

    /// <inheritdoc/>
    public bool Equals(KpiMetricDefinition? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as KpiMetricDefinition);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}

/// <summary>Constant codes matching <see cref="IndicatorCategory"/>, used to avoid a circular static-init dependency.</summary>
internal static class IndicatorCategoryCodes
{
    public const string Lagging = "Lagging";
    public const string Leading = "Leading";
    public const string Cultural = "Cultural";
}
