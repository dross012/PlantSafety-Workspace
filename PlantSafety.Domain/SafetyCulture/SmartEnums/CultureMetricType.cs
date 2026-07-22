// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture
//  Layer:          Domain
//  File:           CultureMetricType.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Smart Enum enumerating the 8 canonical safety-culture KPIs
//                  defined by the PlantSafety Program Structure "Safety Culture
//                  Module" (section 10, "Culture Metrics & KPIs"). These metrics
//                  feed directly into Scorecards and executive dashboards, so
//                  the type is modeled as a Smart Enum (Rule 2.7) rather than a
//                  plain enum.
//                  Additive-only evolution: new metrics may be appended;
//                  existing codes/ids/display names must never change meaning.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.SmartEnums;

/// <summary>
/// Canonical safety-culture KPI. Strong-typed Smart Enum carrying stable
/// <see cref="Id"/>/<see cref="Code"/> pairs plus the metric's preferred trend
/// direction, used to classify culture metric observations feeding Scorecards.
/// </summary>
public sealed class CultureMetricType
{
    /// <summary>Stable numeric identifier (1-based, matches the source document ordering).</summary>
    public int Id { get; }

    /// <summary>Stable machine-readable code (PascalCase, no abbreviations).</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Whether a higher observed value indicates a healthier safety culture for this metric.</summary>
    public bool HigherIsBetter { get; }

    private CultureMetricType(int id, string code, string displayName, bool higherIsBetter)
    {
        Id = id;
        Code = code;
        DisplayName = displayName;
        HigherIsBetter = higherIsBetter;
    }

    /// <summary>Near-miss reporting rate. Higher reporting indicates a stronger just-culture (low reporting suggests underreporting or fear of retaliation).</summary>
    public static readonly CultureMetricType NearMissReportingRate = new(1, "NearMissReportingRate", "Near-Miss Reporting Rate", higherIsBetter: true);

    /// <summary>Count of employee-initiated stop-work actions.</summary>
    public static readonly CultureMetricType StopWorkActions = new(2, "StopWorkActions", "Stop-Work Actions", higherIsBetter: true);

    /// <summary>PPE compliance rate across observed shifts/areas.</summary>
    public static readonly CultureMetricType PpeComplianceRate = new(3, "PpeComplianceRate", "PPE Compliance Rate", higherIsBetter: true);

    /// <summary>Behavioral safety observation program completion rate.</summary>
    public static readonly CultureMetricType BehavioralObservationCompletion = new(4, "BehavioralObservationCompletion", "Behavioral Observation Completion", higherIsBetter: true);

    /// <summary>Frequency of visible leadership engagement in safety walk-arounds and audits.</summary>
    public static readonly CultureMetricType LeadershipEngagementFrequency = new(5, "LeadershipEngagementFrequency", "Leadership Engagement Frequency", higherIsBetter: true);

    /// <summary>Safety training completion rate.</summary>
    public static readonly CultureMetricType TrainingCompletionRate = new(6, "TrainingCompletionRate", "Training Completion Rate", higherIsBetter: true);

    /// <summary>Employee participation rate in safety meetings.</summary>
    public static readonly CultureMetricType SafetyMeetingParticipation = new(7, "SafetyMeetingParticipation", "Employee Participation in Safety Meetings", higherIsBetter: true);

    /// <summary>Volume of employee-submitted safety suggestions.</summary>
    public static readonly CultureMetricType SafetySuggestionSubmissions = new(8, "SafetySuggestionSubmissions", "Safety Suggestion Submissions", higherIsBetter: true);

    /// <summary>Default metric used when no explicit metric has been assigned.</summary>
    public static readonly CultureMetricType Default = NearMissReportingRate;

    /// <summary>All defined culture metrics, in canonical (source-document) order.</summary>
    public static readonly IReadOnlyList<CultureMetricType> All =
    [
        NearMissReportingRate,
        StopWorkActions,
        PpeComplianceRate,
        BehavioralObservationCompletion,
        LeadershipEngagementFrequency,
        TrainingCompletionRate,
        SafetyMeetingParticipation,
        SafetySuggestionSubmissions,
    ];

    /// <summary>Resolves a <see cref="CultureMetricType"/> from its <see cref="Code"/>. Throws if not found.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known metric.</exception>
    public static CultureMetricType From(string code)
        => TryParse(code, out var value)
            ? value!
            : throw new ArgumentException($"'{code}' is not a valid CultureMetricType code.", nameof(code));

    /// <summary>Attempts to resolve a <see cref="CultureMetricType"/> from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out CultureMetricType? value)
    {
        value = All.FirstOrDefault(m => string.Equals(m.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognised metric code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
