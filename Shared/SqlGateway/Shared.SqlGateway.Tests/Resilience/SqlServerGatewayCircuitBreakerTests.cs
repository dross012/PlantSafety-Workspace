// ============================================================================
//  Project:        Shared.SqlGateway.Tests
//  File:           SqlServerGatewayCircuitBreakerTests.cs
//  Created:        2026-08-09
//  Description:    Tests the circuit-breaker fail-fast path without a real
//                  SQL Server (none exists in this sandbox): trips
//                  SqlCircuitBreaker open directly, then asserts
//                  QueryAsync/QueryMultipleAsync throw immediately without
//                  ever attempting a connection, rather than exercising a
//                  real SqlConnection.
// ============================================================================
using AwesomeAssertions;

using Microsoft.Extensions.Options;

using Shared.Deterministic.Abstractions.Clock;
using Shared.SqlGateway.Infrastructure;
using Shared.SqlGateway.Infrastructure.Options;
using Shared.SqlGateway.Infrastructure.Resilience;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SqlGateway.Tests.Resilience;

public sealed class SqlServerGatewayCircuitBreakerTests
{
    private sealed class NoOpLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private sealed class FixedCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation";
    }

    private static SqlServerGateway CreateGateway(SqlCircuitBreaker circuitBreaker) =>
        new(
            Options.Create(new SqlGatewayOptions { ConnectionString = "Server=unused;Database=unused;" }),
            new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new NoOpLogEventWriter(),
            new FixedCorrelationContext(),
            circuitBreaker);

    [Fact]
    public async Task QueryAsync_CircuitOpen_ThrowsWithoutAttemptingAConnection()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var circuitBreaker = new SqlCircuitBreaker(clock);
        for (int i = 0; i < 5; i++)
        {
            circuitBreaker.Breaker.RecordFailure();
        }

        SqlServerGateway gateway = CreateGateway(circuitBreaker);

        Func<Task> act = () => gateway.QueryAsync<int>("dbo.SomeProcedure", new { });

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*circuit breaker is open*");
    }

    [Fact]
    public async Task QueryMultipleAsync_CircuitOpen_ThrowsWithoutAttemptingAConnection()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var circuitBreaker = new SqlCircuitBreaker(clock);
        for (int i = 0; i < 5; i++)
        {
            circuitBreaker.Breaker.RecordFailure();
        }

        SqlServerGateway gateway = CreateGateway(circuitBreaker);

        Func<Task> act = () => gateway.QueryMultipleAsync("dbo.SomeProcedure", new { });

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*circuit breaker is open*");
    }
}
