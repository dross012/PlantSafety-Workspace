// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           GlobalErrorHandlerTests.cs
//  Created:        2026-08-09
//  Description:    Direct tests for GlobalErrorHandler.HandleAsync (Phase
//                  2.3) using a real DefaultHttpContext with a simulated
//                  IExceptionHandlerPathFeature -- faster and less brittle
//                  than hunting for a real endpoint that happens to throw
//                  an unhandled exception, while still exercising the real
//                  ErrorEnvelopeFactory/ExceptionClassifier/StandardErrorPolicy
//                  pipeline end-to-end.
// ============================================================================
using System.Text.Json;

using AwesomeAssertions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Envelope;
using Shared.ErrorHandling.Abstractions.Policy;
using Shared.Telemetry.Abstractions.Events;

using PlantSafety.UI.Api.Middleware;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class GlobalErrorHandlerTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }

        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static DefaultHttpContext CreateContext(Exception exception, string path, string? correlationId = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        if (correlationId is not null)
        {
            context.Request.Headers["X-Correlation-Id"] = correlationId;
        }

        context.Features.Set<IExceptionHandlerPathFeature>(new ExceptionHandlerFeature { Error = exception, Path = path });
        return context;
    }

    private static (IClock Clock, IErrorPolicy Policy, CircuitBreaker Breaker) CreatePolicyStack()
    {
        var clock = new FixedClock(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var breaker = new CircuitBreaker(CircuitBreakerPolicy.Standard, clock);
        var policy = new StandardErrorPolicy(ErrorRetryPolicy.Standard, breaker);
        return (clock, policy, breaker);
    }

    [Fact]
    public async Task HandleAsync_ValidationException_Returns400WithErrorEnvelope()
    {
        DefaultHttpContext context = CreateContext(new ArgumentException("bad input"), "/api/v1/ui/facilities", correlationId: "corr-123");
        (IClock clock, IErrorPolicy policy, CircuitBreaker breaker) = CreatePolicyStack();

        await GlobalErrorHandler.HandleAsync(context, clock, policy, breaker, new CapturingLogEventWriter());

        context.Response.StatusCode.Should().Be(400);
        context.Response.Body.Position = 0;
        ErrorEnvelope? envelope = await JsonSerializer.DeserializeAsync<ErrorEnvelope>(context.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        envelope.Should().NotBeNull();
        envelope!.Category.Should().Be(ErrorCategory.Validation);
        envelope.CorrelationId.Should().Be("corr-123");
        envelope.Module.Should().Be("facilities");
    }

    [Fact]
    public async Task HandleAsync_NoCorrelationIdHeader_GeneratesOne()
    {
        DefaultHttpContext context = CreateContext(new Exception("boom"), "/api/v1/ui/engines/severity-assessment/assess");
        (IClock clock, IErrorPolicy policy, CircuitBreaker breaker) = CreatePolicyStack();

        await GlobalErrorHandler.HandleAsync(context, clock, policy, breaker, new CapturingLogEventWriter());

        context.Response.Body.Position = 0;
        ErrorEnvelope? envelope = await JsonSerializer.DeserializeAsync<ErrorEnvelope>(context.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        envelope!.CorrelationId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(envelope.CorrelationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_EnginesRoute_ExtractsTheEngineNameAsModule()
    {
        DefaultHttpContext context = CreateContext(new Exception("boom"), "/api/v1/ui/engines/severity-assessment/assess");
        (IClock clock, IErrorPolicy policy, CircuitBreaker breaker) = CreatePolicyStack();

        await GlobalErrorHandler.HandleAsync(context, clock, policy, breaker, new CapturingLogEventWriter());

        context.Response.Body.Position = 0;
        ErrorEnvelope? envelope = await JsonSerializer.DeserializeAsync<ErrorEnvelope>(context.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        envelope!.Module.Should().Be("severity-assessment");
    }

    [Fact]
    public async Task HandleAsync_DependencyException_SetsRetryAfterHeader()
    {
        DefaultHttpContext context = CreateContext(new HttpRequestException("downstream unavailable"), "/api/v1/ui/facilities");
        (IClock clock, IErrorPolicy policy, CircuitBreaker breaker) = CreatePolicyStack();

        await GlobalErrorHandler.HandleAsync(context, clock, policy, breaker, new CapturingLogEventWriter());

        context.Response.StatusCode.Should().Be(502);
        context.Response.Headers.RetryAfter.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleAsync_InternalException_RecordsACircuitBreakerFailure()
    {
        DefaultHttpContext context = CreateContext(new Exception("mystery"), "/api/v1/ui/facilities");
        (IClock clock, IErrorPolicy policy, CircuitBreaker breaker) = CreatePolicyStack();

        for (var i = 0; i < CircuitBreakerPolicy.Standard.FailureThreshold; i++)
        {
            await GlobalErrorHandler.HandleAsync(CreateContext(new Exception("mystery"), "/api/v1/ui/facilities"), clock, policy, breaker, new CapturingLogEventWriter());
        }

        breaker.State.Should().Be(CircuitState.Open);
    }

    [Fact]
    public async Task HandleAsync_WritesAStructuredLogEventWithTheCorrelationIdAndClassifiedCategory()
    {
        DefaultHttpContext context = CreateContext(new ArgumentException("bad input"), "/api/v1/ui/facilities", correlationId: "corr-999");
        (IClock clock, IErrorPolicy policy, CircuitBreaker breaker) = CreatePolicyStack();
        var writer = new CapturingLogEventWriter();

        await GlobalErrorHandler.HandleAsync(context, clock, policy, breaker, writer);

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.CorrelationId.Should().Be("corr-999");
        writer.LastEvent.Category.Should().Be(LogCategory.Facility);
        writer.LastEvent.Severity.Should().Be(LogSeverity.Error);
    }
}
