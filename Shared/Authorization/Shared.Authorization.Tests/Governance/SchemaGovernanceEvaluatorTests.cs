// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           SchemaGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class SchemaGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public void EvaluateAndReport_AllValid_WritesNoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var callerResolution = new CallerContextResolution(Caller, ApiKeyIdentity.NoKeyProvided(EnterpriseRole.PlatformAdmin));
        var tenantContext = new TenantContext("acme-plant-1", null, Caller, "corr-1");
        var tenantResolution = new TenantContextResolution(tenantContext, false, false);
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Clock.UtcNow);

        SchemaGovernanceEvaluator.EvaluateAndReport(callerResolution, tenantResolution, metadata, Clock, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateAndReport_AllMissing_WritesSchemaGovernanceViolationLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        IReadOnlyList<string> violations = SchemaGovernanceEvaluator.EvaluateAndReport(null, null, null, Clock, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("SCHEMA_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
    }
}
