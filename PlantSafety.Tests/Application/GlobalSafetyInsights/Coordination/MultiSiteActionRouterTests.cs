// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.GlobalSafetyInsights.Coordination
//  Layer:          Tests (Unit)
//  File:           MultiSiteActionRouterTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for MultiSiteActionRouter's validate -> policy -> route -> audit
//                  sequence: residency rejection, no-eligible-sites rejection, immediate dispatch +
//                  audit for a non-approval action, and approval-required deferral (both with and
//                  without a configured approval WorkflowDefinition). Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using MediatR;
using PlantSafety.Application.Enterprise.MultiRegion;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Application.GlobalSafetyInsights.Commands;
using PlantSafety.Application.GlobalSafetyInsights.Coordination;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Models;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Publishers;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;
using PlantSafety.Domain.GlobalSafetyInsights.Events;
using PlantSafety.Domain.MultiRegion.DataResidency;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Tests.Application.GlobalSafetyInsights.Coordination;

/// <summary>Unit tests for <see cref="MultiSiteActionRouter"/>.</summary>
public sealed class MultiSiteActionRouterTests : UnitTestBase
{
    private readonly ITenantRegionRouter _regionRouter = Substitute.For<ITenantRegionRouter>();
    private readonly IDataResidencyEnforcer _residencyEnforcer = Substitute.For<IDataResidencyEnforcer>();
    private readonly IMultiSiteSafetyPolicy _policy = Substitute.For<IMultiSiteSafetyPolicy>();
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ICrossSiteActionEventPublisher _publisher = Substitute.For<ICrossSiteActionEventPublisher>();
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository = Substitute.For<IWorkflowDefinitionRepository>();
    private readonly IWorkflowInstanceFactory _workflowInstanceFactory = Substitute.For<IWorkflowInstanceFactory>();
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository = Substitute.For<IWorkflowInstanceRepository>();

    public MultiSiteActionRouterTests()
    {
        // Region resolution is unconditionally exercised on every RouteAsync call (Phase 8 §6
        // governance-validation fix: the resolved region is now threaded into the audit trail
        // instead of being discarded) -- stub a default so tests that don't care about region
        // routing specifically still exercise a real, non-null Region.
        _regionRouter.ResolveRegion(Arg.Any<Guid>()).Returns(Region.UsEast);
    }

    private MultiSiteActionRouter Sut() => new(
        _regionRouter, _residencyEnforcer, _policy, _sender, _publisher,
        _workflowDefinitionRepository, _workflowInstanceFactory, _workflowInstanceRepository,
        NullLogger<MultiSiteActionRouter>());

    private static MultiSiteCoordinationDecision EligibleDecision(IReadOnlyList<Guid> siteIds, bool requiresApproval = false)
        => new()
        {
            EligibleSiteIds = siteIds,
            IneligibleSites = [],
            RequiresApproval = requiresApproval,
            ApprovalReason = requiresApproval ? "test reason" : null,
        };

    [Fact]
    public async Task RouteAsync_WhenResidencyViolated_ReturnsFailureAndSkipsPolicy()
    {
        var site = Guid.NewGuid();
        _residencyEnforcer
            .When(x => x.EnsureSatisfied(TestTenantId.Value, Arg.Any<CorrelationId>(), Arg.Any<CausationId>()))
            .Throw(new DataResidencyViolationException(
                TestTenantId.Value,
                DataResidencyPolicy.Default,
                Region.Default,
                TestCorrelationId,
                TestCausationId));

        var request = new MultiSiteActionRequest(
            TestTenantId.Value, [site], CrossSiteActionType.IncidentEscalation, "desc", null, null,
            TestCorrelationId.Value, TestCausationId.Value, IncidentEscalationReason: "reason");

        var result = await Sut().RouteAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("GlobalSafetyInsights.Coordination.ResidencyViolation");
        _policy.DidNotReceiveWithAnyArgs().Evaluate(default, default!, default, default);
    }

    [Fact]
    public async Task RouteAsync_WhenNoSitesEligible_ReturnsFailure()
    {
        _policy.Evaluate(TestTenantId.Value, Arg.Any<IReadOnlyList<Guid>>(), CrossSiteActionType.IncidentEscalation, Arg.Any<IReadOnlyList<Guid>?>())
            .Returns(EligibleDecision([]));

        var request = new MultiSiteActionRequest(
            TestTenantId.Value, [Guid.Empty], CrossSiteActionType.IncidentEscalation, "desc", null, null,
            TestCorrelationId.Value, TestCausationId.Value, IncidentEscalationReason: "reason");

        var result = await Sut().RouteAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("GlobalSafetyInsights.Coordination.NoEligibleSites");
    }

    [Fact]
    public async Task RouteAsync_WhenNoApprovalRequired_DispatchesPublishesAndAudits()
    {
        var site = Guid.NewGuid();
        _policy.Evaluate(TestTenantId.Value, Arg.Any<IReadOnlyList<Guid>>(), CrossSiteActionType.IncidentEscalation, Arg.Any<IReadOnlyList<Guid>?>())
            .Returns(EligibleDecision([site]));

        _sender.Send(Arg.Any<CrossSiteIncidentEscalationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<CrossSiteIncidentEscalationResult>.Success(
                new CrossSiteIncidentEscalationResult([new CrossSiteIncidentSiteOutcome(site, true, Guid.NewGuid(), "Escalated", null)])));

        _sender.Send(Arg.Any<RecordCrossSiteActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok);

        var request = new MultiSiteActionRequest(
            TestTenantId.Value, [site], CrossSiteActionType.IncidentEscalation, "desc", null, null,
            TestCorrelationId.Value, TestCausationId.Value, IncidentEscalationReason: "reason");

        var result = await Sut().RouteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Executed.Should().BeTrue();
        result.Value.RequiresApproval.Should().BeFalse();
        result.Value.DispatchSucceeded.Should().BeTrue();

        await _sender.Received(1).Send(Arg.Any<CrossSiteIncidentEscalationCommand>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(Arg.Any<CrossSiteActionEvent>(), Arg.Any<CancellationToken>());

        // AuditAsync publishes CrossSiteActionEvent only -- the idempotent CrossSiteActionConsumer
        // is the sole authoritative writer of RecordCrossSiteActionCommand (Phase 8 §6
        // governance-validation fix: a prior version double-wrote the audit ledger here).
        await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<RecordCrossSiteActionCommand>(), Arg.Any<CancellationToken>());
        await _workflowInstanceRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task RouteAsync_WhenApprovalRequiredAndDefinitionConfigured_StartsWorkflowInstanceAndSkipsDispatch()
    {
        var site = Guid.NewGuid();
        _policy.Evaluate(TestTenantId.Value, Arg.Any<IReadOnlyList<Guid>>(), CrossSiteActionType.CorrectiveActionRollout, Arg.Any<IReadOnlyList<Guid>?>())
            .Returns(EligibleDecision([site], requiresApproval: true));

        var definition = WorkflowDefinition.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null, WorkflowType.Approval, MultiSiteActionRouter.ApprovalWorkflowDefinitionName, "test");
        definition.AddStep(
            1, "Approve", WorkflowStepType.HumanApproval, AssignedActor.ForRole("SafetyManager"), AssignedActor.ForRole("SafetyManager"),
            GuardCondition.None, null, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        definition.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        _workflowDefinitionRepository.ListActiveAsync(TestTenantId.Value, Arg.Any<CancellationToken>())
            .Returns([definition]);

        var instance = WorkflowInstance.Start(
            definition.WorkflowDefinitionId, WorkflowType.Approval, definition.GetStartStep().StepId, WorkflowStepType.HumanApproval,
            AssignedActor.ForRole("SafetyManager"), TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null);

        _workflowInstanceFactory
            .Start(definition, Arg.Any<UserId>(), Arg.Any<UtcTimestamp>(), Arg.Any<CorrelationId>(), Arg.Any<CausationId>(),
                Arg.Any<PerformedByActor>(), TestTenantId.Value, null, Arg.Any<IReadOnlyDictionary<string, string>?>())
            .Returns(instance);

        _sender.Send(Arg.Any<RecordCrossSiteActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok);

        var request = new MultiSiteActionRequest(
            TestTenantId.Value, [site], CrossSiteActionType.CorrectiveActionRollout, "desc", null, null,
            TestCorrelationId.Value, TestCausationId.Value,
            CorrectiveAction: new CrossSiteCorrectiveActionPayload("title", "High", Guid.NewGuid(), "GlobalSafetyAnomaly", DateTimeOffset.UtcNow.AddDays(7), Guid.NewGuid()));

        var result = await Sut().RouteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresApproval.Should().BeTrue();
        result.Value.Executed.Should().BeFalse();
        result.Value.WorkflowInstanceId.Should().Be(instance.WorkflowInstanceId.Value);

        await _workflowInstanceRepository.Received(1).AddAsync(instance, Arg.Any<CancellationToken>());
        await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<CrossSiteCorrectiveActionCommand>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(Arg.Any<CrossSiteActionEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RouteAsync_WhenApprovalRequiredButNoDefinitionConfigured_DefersWithoutWorkflowInstance()
    {
        var site = Guid.NewGuid();
        _policy.Evaluate(TestTenantId.Value, Arg.Any<IReadOnlyList<Guid>>(), CrossSiteActionType.LockoutPropagation, Arg.Any<IReadOnlyList<Guid>?>())
            .Returns(EligibleDecision([site], requiresApproval: true));

        _workflowDefinitionRepository.ListActiveAsync(TestTenantId.Value, Arg.Any<CancellationToken>())
            .Returns([]);

        _sender.Send(Arg.Any<RecordCrossSiteActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok);

        var request = new MultiSiteActionRequest(
            TestTenantId.Value, [site], CrossSiteActionType.LockoutPropagation, "desc", null, null,
            TestCorrelationId.Value, TestCausationId.Value, LockoutPropagationReason: "reason");

        var result = await Sut().RouteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresApproval.Should().BeTrue();
        result.Value.Executed.Should().BeFalse();
        result.Value.WorkflowInstanceId.Should().BeNull();

        await _workflowInstanceRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);

        // AuditAsync publishes CrossSiteActionEvent only -- see the comment in the prior test.
        await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<RecordCrossSiteActionCommand>(), Arg.Any<CancellationToken>());
    }
}
