// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ResourceAvailabilityModel.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Domain service that answers resource-availability questions (remaining
//                  capacity, free windows) over an in-memory set of ResourceAvailabilityWindow
//                  and already-committed ScheduleAssignment records. Pure computation only;
//                  no infrastructure or persistence access. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;

/// <summary>Domain service that computes resource availability and remaining capacity.</summary>
public sealed class ResourceAvailabilityModel : IDomainService
{
    /// <summary>
    /// Returns the number of concurrent task slots still free for <paramref name="resourceId"/>
    /// during [<paramref name="startUtc"/>, <paramref name="endUtc"/>), given its declared
    /// availability windows and the assignments already committed against it.
    /// </summary>
    public int RemainingCapacity(
        SchedulingResourceId resourceId,
        UtcTimestamp startUtc,
        UtcTimestamp endUtc,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments)
    {
        var window = availability.FirstOrDefault(w =>
            w.ResourceId == resourceId && w.Contains(startUtc, endUtc));

        if (window is null)
            return 0;

        var overlappingCount = committedAssignments.Count(a =>
            a.ResourceId == resourceId &&
            a.StartUtc.Value < endUtc.Value &&
            startUtc.Value < a.EndUtc.Value);

        return Math.Max(0, window.Capacity - overlappingCount);
    }

    /// <summary>True when <paramref name="resourceId"/> has at least one free slot for the requested window.</summary>
    public bool IsAvailable(
        SchedulingResourceId resourceId,
        UtcTimestamp startUtc,
        UtcTimestamp endUtc,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments)
        => RemainingCapacity(resourceId, startUtc, endUtc, availability, committedAssignments) > 0;

    /// <summary>
    /// Enumerates candidate start times for <paramref name="durationUtc"/> within the resource's
    /// declared windows, stepping every <paramref name="stepMinutes"/> minutes, restricted to the
    /// range [<paramref name="earliestStartUtc"/>, <paramref name="latestFinishUtc"/>).
    /// </summary>
    public IEnumerable<UtcTimestamp> EnumerateCandidateStartTimes(
        SchedulingResourceId resourceId,
        TimeSpan durationUtc,
        UtcTimestamp earliestStartUtc,
        UtcTimestamp latestFinishUtc,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments,
        int stepMinutes = 30)
    {
        if (stepMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(stepMinutes), "stepMinutes must be at least 1.");

        var step = TimeSpan.FromMinutes(stepMinutes);

        foreach (var window in availability.Where(w => w.ResourceId == resourceId))
        {
            var candidateStart = window.StartUtc.Value > earliestStartUtc.Value
                ? window.StartUtc.Value
                : earliestStartUtc.Value;
            var windowEnd = window.EndUtc.Value < latestFinishUtc.Value
                ? window.EndUtc.Value
                : latestFinishUtc.Value;

            while (candidateStart + durationUtc <= windowEnd)
            {
                var start = UtcTimestamp.From(candidateStart);
                var end = UtcTimestamp.From(candidateStart + durationUtc);

                if (IsAvailable(resourceId, start, end, availability, committedAssignments))
                    yield return start;

                candidateStart += step;
            }
        }
    }
}
