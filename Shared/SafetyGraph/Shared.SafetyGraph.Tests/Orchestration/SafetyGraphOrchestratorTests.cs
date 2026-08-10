// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphOrchestratorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyGraph.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyGraph.Tests.Orchestration;

public sealed class SafetyGraphOrchestratorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    private static SafetyGraphEvaluationRequestContract BuildRequest(string facilityName = "Plant One") => new(
        new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        new SafetyGraphConstructionRequestContract(
            new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
            Guid.NewGuid(), facilityName, null, Guid.NewGuid(), null, null, null, null));

    [Fact]
    public async Task EvaluateAsync_ValidRequest_ReturnsResultWithMatchingTenant()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        SafetyGraphEvaluationResultContract result = await SafetyGraphOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        result.Metadata.TenantId.Should().Be("acme-plant-1");
        result.Graph.Nodes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_EmitsOrchestrationCompletedAndEvaluationEventPublished()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        await SafetyGraphOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        writer.Events.Should().Contain(e => e.EventId == "SAFETY_GRAPH_ORCHESTRATION_COMPLETED");
        writer.Events.Should().Contain(e => e.EventId == "SAFETY_GRAPH_EVALUATION_EVENT_PUBLISHED");
        writer.Events.Should().Contain(e => e.EventId == "SAFETY_GRAPH_CONSTRUCTED");
        writer.Events.Should().Contain(e => e.EventId == "SAFETY_GRAPH_INFERENCE_COMPUTED");
    }

    [Fact]
    public async Task EvaluateAsync_EmptyFacilityName_ThrowsSafetyGraphValidationException()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        Func<Task> act = () => SafetyGraphOrchestrator.EvaluateAsync(
            BuildRequest(facilityName: string.Empty), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        (await act.Should().ThrowAsync<SafetyGraphValidationException>())
            .Which.ErrorCode.Should().Be(SafetyGraphErrors.MissingFacilityName);
    }

    [Fact]
    public async Task EvaluateAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");

        Func<Task> act = () => SafetyGraphOrchestrator.EvaluateAsync(
            BuildRequest(), deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
