/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               PredictionHorizon.cs
    DESCRIPTION:        Value Object representing the forward-looking time window for a safety prediction.
                        The horizon defines how far into the future the prediction is relevant.
                        Must be a positive duration. (Rule 2.8)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.PredictiveSafety.ValueObjects;

/// <summary>
/// The forward-looking time window for a safety prediction. Defines how far into the future
/// the prediction is asserted to be relevant.
/// </summary>
public sealed class PredictionHorizon : ValueObject
{
    /// <summary>The number of hours in the prediction horizon. Always positive.</summary>
    public int Hours { get; }

    /// <summary>A human-readable description of the horizon (e.g., "24 hours", "7 days").</summary>
    public string Description { get; }

    private PredictionHorizon(int hours, string description)
    {
        Hours = hours;
        Description = description;
    }

    /// <summary>
    /// Creates a <see cref="PredictionHorizon"/> from a number of hours.
    /// </summary>
    /// <param name="hours">The number of hours. Must be positive.</param>
    /// <returns>A validated <see cref="PredictionHorizon"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when hours is not positive.</exception>
    public static PredictionHorizon FromHours(int hours)
    {
        if (hours <= 0)
            throw new ArgumentOutOfRangeException(nameof(hours),
                $"PredictionHorizon must be a positive number of hours. Received: {hours}.");

        var description = hours switch
        {
            <= 1 => "1 hour",
            <= 24 => $"{hours} hours",
            <= 168 => $"{hours / 24} day{(hours / 24 == 1 ? "" : "s")}",
            _ => $"{hours / 168} week{(hours / 168 == 1 ? "" : "s")}"
        };

        return new PredictionHorizon(hours, description);
    }

    /// <summary>Standard horizon: 24 hours.</summary>
    public static PredictionHorizon TwentyFourHours => FromHours(24);

    /// <summary>Standard horizon: 7 days.</summary>
    public static PredictionHorizon SevenDays => FromHours(168);

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hours;
    }

    /// <inheritdoc />
    public override string ToString() => Description;
}
