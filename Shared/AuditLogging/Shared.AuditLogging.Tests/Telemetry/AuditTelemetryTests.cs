// ============================================================================
//  Project:        Shared.AuditLogging.Tests
//  File:           AuditTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;
using Shared.AuditLogging.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.AuditLogging.Tests.Telemetry;

public sealed class AuditTelemetryTests
{
    private sealed class FakeAuditLogRepository(AuditLogSearchResult? result, Exception? throwOnSearch) : IAuditLogRepository
    {
        public Task<AuditLogSearchResult> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken) =>
            throwOnSearch is not null ? throw throwOnSearch : Task.FromResult(result!);
    }

    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private sealed class FixedCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation";
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task SearchAsync_Success_ReturnsInnerResultAndLogsAuditSearchExecuted()
    {
        var expected = new AuditLogSearchResult([], PageNumber: 1, PageSize: 50, TotalCount: 0);
        var writer = new CapturingLogEventWriter();
        var repository = new TelemetryAuditLogRepository(
            new FakeAuditLogRepository(expected, null), Clock, writer, new FixedCorrelationContext());

        AuditLogSearchResult result = await repository.SearchAsync(new AuditLogSearchCriteria(), CancellationToken.None);

        result.Should().Be(expected);
        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("AUDIT_SEARCH_EXECUTED");
        writer.LastEvent.Category.Should().Be(LogCategory.Audit);
        writer.LastEvent.CorrelationId.Should().Be("test-correlation");
    }

    [Fact]
    public async Task SearchAsync_InnerRepositoryThrows_RethrowsWithoutSwallowing()
    {
        var writer = new CapturingLogEventWriter();
        var repository = new TelemetryAuditLogRepository(
            new FakeAuditLogRepository(null, new InvalidOperationException("db unavailable")),
            Clock, writer, new FixedCorrelationContext());

        Func<Task> act = () => repository.SearchAsync(new AuditLogSearchCriteria(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("db unavailable");
    }
}
