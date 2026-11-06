// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveSafetyEvaluationPipelineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Orchestration;

public sealed class PredictiveSafetyEvaluationPipelineTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static PredictiveSafetyEvaluationRequestContract BuildRequest() => new(
        Metadata: new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        EventType: "ChemicalSpill",
        EventDescription: "Spill in loading dock",
        EmployeeExposureCount: 1,
        MachineFailureCount: 0,
        EventChainLength: 1,
        RoutingResult: null,
        MachineId: Guid.NewGuid(),
        MaintenanceProfile: null,
        RecentMachineFailureCount: 0,
        SchedulingDecision: null,
        RecentReliabilityFailureCount: 0,
        TotalLogEventCount: 0,
        WarningOrAboveLogEventCount: 0,
        DistinctEventIdCount: 0,
        GovernanceViolationCount: 0,
        AsOfUtc: Clock.UtcNow);

    [Fact]
    public async Task EvaluateAsync_ComposesAllFiveDomainDecisions()
    {
        var writer = new CapturingLogEventWriter();

        PredictiveSafetyEvaluationResultContract result = await PredictiveSafetyEvaluationPipeline.EvaluateAsync(BuildRequest(), Clock, writer);

        result.Hazard.Should().NotBeNull();
        result.Maintenance.Should().NotBeNull();
        result.Routing.Should().NotBeNull();
        result.Scheduling.Should().NotBeNull();
        result.Audit.Should().NotBeNull();
    }

    [Fact]
    public async Task EvaluateAsync_OverallPriorityIsWorstCaseAcrossDomains()
    {
        var writer = new CapturingLogEventWriter();
        // Force maintenance escalation via an overdue profile, which should drive OverallPriority above Low.
        PredictiveSafetyEvaluationRequestContract request = BuildRequest() with
        {
            MaintenanceProfile = new Shared.MachineDomain.Abstractions.Maintenance.MachineMaintenanceProfile(
                LastMaintenanceAtUtc: Clock.UtcNow.AddDays(-90),
                NextScheduledMaintenanceAtUtc: Clock.UtcNow.AddDays(-1),
                MaintenanceIntervalDays: 30,
                UpdatedAtUtc: Clock.UtcNow.AddDays(-90)),
        };

        PredictiveSafetyEvaluationResultContract result = await PredictiveSafetyEvaluationPipeline.EvaluateAsync(request, Clock, writer);

        result.Maintenance.RequiresImmediateMaintenance.Should().BeTrue();
        result.OverallPriority.Should().NotBe(PredictivePriority.Low);
    }

    [Fact]
    public async Task EvaluateAsync_NullRequest_Throws()
    {
        var writer = new CapturingLogEventWriter();

        Func<Task> act = () => PredictiveSafetyEvaluationPipeline.EvaluateAsync(null!, Clock, writer);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
