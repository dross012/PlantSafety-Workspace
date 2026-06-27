/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelAccuracyScore.cs
    DESCRIPTION:        Value Object: primary accuracy metric of a trained ML model. Range [0.0, 1.0].
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.AITraining.ValueObjects;

/// <summary>Primary accuracy metric score for a trained ML model. Range: [0.0, 1.0].</summary>
public sealed class ModelAccuracyScore : ValueObject
{
    /// <summary>The numeric accuracy score in [0.0, 1.0].</summary>
    public double Score { get; }

    /// <summary>The name of the metric this score represents (e.g., "F1", "AUC").</summary>
    public string MetricName { get; }

    /// <summary>Whether this score meets the minimum deployment threshold (Safety Rule 4.4).</summary>
    public bool MeetsDeploymentThreshold => Score >= IntelligenceConstants.MinimumConfidenceScore;

    private ModelAccuracyScore(double score, string metricName)
    {
        Score = score;
        MetricName = metricName;
    }

    /// <summary>Creates a <see cref="ModelAccuracyScore"/> from a score and metric name.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when score is outside [0.0, 1.0].</exception>
    /// <exception cref="ArgumentException">Thrown when metricName is empty.</exception>
    public static ModelAccuracyScore From(double score, string metricName)
    {
        if (score is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(score),
                $"ModelAccuracyScore must be between 0.0 and 1.0. Received: {score}.");
        if (string.IsNullOrWhiteSpace(metricName))
            throw new ArgumentException("MetricName cannot be empty.", nameof(metricName));
        return new ModelAccuracyScore(score, metricName.Trim());
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Score;
        yield return MetricName;
    }

    /// <inheritdoc />
    public override string ToString() => $"{MetricName}={Score:F4}";
}
