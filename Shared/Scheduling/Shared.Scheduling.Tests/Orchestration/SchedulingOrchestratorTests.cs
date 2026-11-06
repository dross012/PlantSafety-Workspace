// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingOrchestratorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Orchestration;

public sealed class SchedulingOrchestratorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    private static SchedulingEvaluationRequestContract BuildRequest(int dueWithinHours = 6, int overdueActionCount = 0)
    {
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow);
        var actions = new[] { new RequiredAction("ACT-1", "Action", dueWithinHours) };
        return new SchedulingEvaluationRequestContract(metadata, actions, overdueActionCount, ActiveMachineGroupCount: 0, AsOfUtc: Clock.UtcNow);
    }

    [Fact]
    public async Task EvaluateAsync_ValidRequest_ReturnsResultContractWithMatchingMetadata()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        SchedulingEvaluationResultContract result = await SchedulingOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        result.Metadata.TenantId.Should().Be("acme-plant-1");
        result.Schedule.RuleDecision.TotalRequiredActionCount.Should().Be(1);
    }

    [Fact]
    public async Task EvaluateAsync_ImmediateAction_ReturnsOverdueStatus()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        SchedulingEvaluationResultContract result = await SchedulingOrchestrator.EvaluateAsync(
            BuildRequest(dueWithinHours: 0), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        result.Status.Should().Be(SchedulingDueTimeStatus.Overdue);
        result.Schedule.RuleDecision.RequiresImmediateScheduling.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_EmitsOrchestrationCompletedAndScheduledEventPublished()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        await SchedulingOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        writer.Events.Should().Contain(e => e.EventId == "SCHEDULING_ORCHESTRATION_COMPLETED");
        writer.Events.Should().Contain(e => e.EventId == "SCHEDULING_SCHEDULED_EVENT_PUBLISHED");
        writer.Events.Should().Contain(e => e.EventId == "SCHEDULING_SCHEDULE_COMPUTED");
    }

    [Fact]
    public async Task EvaluateAsync_EmptyRequiredActions_ThrowsSchedulingValidationException()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow);
        var request = new SchedulingEvaluationRequestContract(metadata, Array.Empty<RequiredAction>(), 0, 0, Clock.UtcNow);

        Func<Task> act = () => SchedulingOrchestrator.EvaluateAsync(
            request, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        (await act.Should().ThrowAsync<SchedulingValidationException>())
            .Which.ErrorCode.Should().Be(SchedulingErrors.NoRequiredActionsSupplied);
    }

    [Fact]
    public async Task EvaluateAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");
        var denyingResolver = new AlwaysDenyPermissionResolver();

        Func<Task> act = () => SchedulingOrchestrator.EvaluateAsync(
            BuildRequest(), deniedCaller, tenantContext, requestedFacilityId: null, denyingResolver, tenantFacilityAccessResolver: null, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
