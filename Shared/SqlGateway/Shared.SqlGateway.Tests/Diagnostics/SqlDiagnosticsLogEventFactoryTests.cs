// ============================================================================
//  Project:        Shared.SqlGateway.Tests
//  File:           SqlDiagnosticsLogEventFactoryTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.SqlGateway.Abstractions.Diagnostics;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SqlGateway.Tests.Diagnostics;

public sealed class SqlDiagnosticsLogEventFactoryTests
{
    [Fact]
    public void ToLogEvent_SuccessfulCall_MapsToInfoSeverity()
    {
        var diagnostics = new SqlCallDiagnostics("dbo.usp_GetHazardSummary", ["TenantId", "SiteId"], TimeSpan.FromMilliseconds(42), RowCount: 10, Succeeded: true, SqlErrorCode: null, ErrorMessage: null);

        LogEvent logEvent = SqlDiagnosticsLogEventFactory.ToLogEvent(diagnostics, "corr-1", DateTime.UtcNow);

        logEvent.Severity.Should().Be(LogSeverity.Info);
        logEvent.EventId.Should().Be("SQL_CALL_SUCCEEDED");
        logEvent.Payload["RowCount"].Should().Be(10);
    }

    [Fact]
    public void ToLogEvent_FailedCall_MapsToErrorSeverityAndIncludesErrorCode()
    {
        var diagnostics = new SqlCallDiagnostics("dbo.usp_GetHazardSummary", ["TenantId"], TimeSpan.FromMilliseconds(15), RowCount: null, Succeeded: false, SqlErrorCode: 547, ErrorMessage: "FK violation");

        LogEvent logEvent = SqlDiagnosticsLogEventFactory.ToLogEvent(diagnostics, "corr-2", DateTime.UtcNow);

        logEvent.Severity.Should().Be(LogSeverity.Error);
        logEvent.EventId.Should().Be("SQL_CALL_FAILED");
        logEvent.Payload["SqlErrorCode"].Should().Be(547);
        logEvent.Message.Should().Contain("FK violation");
    }

    [Fact]
    public void ToLogEvent_AlwaysUsesTheSqlGatewayModuleAndInfrastructureCategory()
    {
        var diagnostics = new SqlCallDiagnostics("dbo.usp_GetSafetyCompliance", [], TimeSpan.Zero, 0, true, null, null);

        LogEvent logEvent = SqlDiagnosticsLogEventFactory.ToLogEvent(diagnostics, "corr-3", DateTime.UtcNow);

        logEvent.Module.Should().Be("SqlGateway");
        logEvent.Category.Should().Be(LogCategory.Infrastructure);
        logEvent.CorrelationId.Should().Be("corr-3");
    }
}
