// ============================================================================
//  Project:        Shared.PredictiveModels
//  Module:         Abstractions.Statistics
//  File:           DeterministicSeriesMath.cs
//  Created:        2026-08-08
//  Description:    Phase C.11 (Path C: Operational Modernization): the
//                  shared deterministic-math primitives every predictive
//                  model in this project is built from -- moving averages,
//                  rolling windows, trend slopes (ordinary least squares),
//                  and z-score anomaly detection, per Path C's explicit
//                  "deterministic methods only" instruction. No ML, no
//                  randomness. Extracted here rather than duplicated across
//                  all five model files, since the same three operations
//                  are used identically by each of them.
// ============================================================================
namespace Shared.PredictiveModels.Abstractions.Statistics;

/// <summary>Deterministic statistical primitives shared by every predictive model in this project.</summary>
public static class DeterministicSeriesMath
{
    /// <summary>The arithmetic mean of the last <paramref name="windowSize"/> values in the series (or all of it, if shorter).</summary>
    /// <exception cref="ArgumentException"><paramref name="values"/> is empty, or <paramref name="windowSize"/> is not positive.</exception>
    public static double MovingAverage(IReadOnlyList<double> values, int windowSize)
    {
        if (values.Count == 0)
        {
            throw new ArgumentException("values must not be empty.", nameof(values));
        }
        if (windowSize <= 0)
        {
            throw new ArgumentException("windowSize must be positive.", nameof(windowSize));
        }

        IEnumerable<double> window = values.TakeLast(Math.Min(windowSize, values.Count));
        return window.Average();
    }

    /// <summary>
    /// The ordinary-least-squares slope of the series against its index (0, 1, 2, ...) -- a
    /// deterministic measure of whether the series is trending up, down, or flat. Returns 0 for
    /// a series with fewer than 2 points (no trend is computable).
    /// </summary>
    public static double TrendSlope(IReadOnlyList<double> values)
    {
        if (values.Count < 2)
        {
            return 0d;
        }

        int n = values.Count;
        double meanX = (n - 1) / 2d;
        double meanY = values.Average();

        double numerator = 0d;
        double denominator = 0d;
        for (int i = 0; i < n; i++)
        {
            double dx = i - meanX;
            numerator += dx * (values[i] - meanY);
            denominator += dx * dx;
        }

        return denominator == 0d ? 0d : numerator / denominator;
    }

    /// <summary>
    /// The z-score of <paramref name="latestValue"/> against the population mean/standard deviation of
    /// <paramref name="priorValues"/>. Returns 0 if fewer than 2 prior values exist. If the prior values
    /// have zero variance (a perfectly flat history), any deviation from that constant is a definite
    /// outlier -- this returns positive/negative infinity (matching <paramref name="latestValue"/>'s
    /// direction) rather than 0, so a spike against flat history is never masked as "no anomaly".
    /// </summary>
    public static double ZScore(IReadOnlyList<double> priorValues, double latestValue)
    {
        if (priorValues.Count < 2)
        {
            return 0d;
        }

        double mean = priorValues.Average();
        double variance = priorValues.Sum(v => (v - mean) * (v - mean)) / priorValues.Count;
        double stdDev = Math.Sqrt(variance);

        if (stdDev == 0d)
        {
            return latestValue == mean ? 0d : latestValue > mean ? double.PositiveInfinity : double.NegativeInfinity;
        }

        return (latestValue - mean) / stdDev;
    }

    /// <summary>True if the z-score's absolute value exceeds <paramref name="threshold"/> (default 2.0 -- roughly the 95% band).</summary>
    public static bool IsAnomalous(double zScore, double threshold = 2.0) => Math.Abs(zScore) > threshold;
}
