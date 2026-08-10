// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveSafetyOrchestratorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Orchestration;

public sealed class PredictiveSafetyOrchestratorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    private static PredictiveSafetyEvaluationRequestContract BuildRequest(string eventType = "ChemicalSpill") => new(
        new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        eventType, null, 0, 0, 0, null, Guid.NewGuid(), null, 0, null, 0, 0, 0, 0, 0, Clock.UtcNow);

    [Fact]
    public async Task EvaluateAsync_ValidRequest_ReturnsResultWithMatchingTenant()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        PredictiveSafetyEvaluationResultContract result = await PredictiveSafetyOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        result.Metadata.TenantId.Should().Be("acme-plant-1");
    }

    [Fact]
    public async Task EvaluateAsync_EmitsOrchestrationCompletedAndEvaluationEventPublished()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        await PredictiveSafetyOrchestrator.EvaluateAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        writer.Events.Should().Contain(e => e.EventId == "PREDICTIVE_ORCHESTRATION_COMPLETED");
        writer.Events.Should().Contain(e => e.EventId == "PREDICTIVE_EVALUATION_EVENT_PUBLISHED");
        writer.Events.Should().Contain(e => e.EventId == "PREDICTIVE_EVALUATION_COMPUTED");
    }

    [Fact]
    public async Task EvaluateAsync_EmptyEventType_ThrowsPredictiveSafetyValidationException()
    {
        var writer = new CapturingLogEventWriter();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        Func<Task> act = () => PredictiveSafetyOrchestrator.EvaluateAsync(
            BuildRequest(eventType: string.Empty), Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, writer);

        (await act.Should().ThrowAsync<PredictiveSafetyValidationException>())
            .Which.ErrorCode.Should().Be(PredictiveSafetyErrors.MissingEventType);
    }

    [Fact]
    public async Task EvaluateAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");

        Func<Task> act = () => PredictiveSafetyOrchestrator.EvaluateAsync(
            BuildRequest(), deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
