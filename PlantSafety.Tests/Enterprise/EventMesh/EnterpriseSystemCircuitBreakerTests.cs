// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Enterprise.EventMesh
//  Layer:          Tests (Unit)
//  File:           EnterpriseSystemCircuitBreakerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 9 §1 Reliability Hardening, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Direct, isolated coverage of EnterpriseSystemCircuitBreaker's state machine
//                  (Closed/Open/HalfOpen transitions), independent of HttpExternalSystemPublisher's
//                  own circuit-breaker integration tests. Deterministic, no I/O.
// ============================================================================
using PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

namespace PlantSafety.Tests.Enterprise.EventMesh;

public sealed class EnterpriseSystemCircuitBreakerTests : UnitTestBase
{
    [Fact]
    public void TryEnter_WhenClosed_AlwaysAdmitsTheCall()
    {
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 3);

        breaker.TryEnter().Should().BeTrue();
        breaker.State.Should().Be(EnterpriseSystemCircuitState.Closed);
    }

    [Fact]
    public void RecordFailure_BelowThreshold_StaysClosed()
    {
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 3);

        breaker.RecordFailure();
        breaker.RecordFailure();

        breaker.State.Should().Be(EnterpriseSystemCircuitState.Closed);
    }

    [Fact]
    public void RecordFailure_ReachingThreshold_OpensCircuit()
    {
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 3);

        breaker.RecordFailure();
        breaker.RecordFailure();
        breaker.RecordFailure();

        breaker.State.Should().Be(EnterpriseSystemCircuitState.Open);
    }

    [Fact]
    public void TryEnter_WhenOpenAndDurationNotElapsed_RejectsTheCall()
    {
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 1, openDuration: TimeSpan.FromMinutes(5));

        breaker.RecordFailure();

        breaker.TryEnter().Should().BeFalse("the open duration has not elapsed yet");
        breaker.State.Should().Be(EnterpriseSystemCircuitState.Open);
    }

    [Fact]
    public void TryEnter_WhenOpenAndDurationElapsed_AdmitsExactlyOneHalfOpenTrial()
    {
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 1, openDuration: TimeSpan.Zero);

        breaker.RecordFailure();

        breaker.TryEnter().Should().BeTrue("the open duration has already elapsed, admitting a half-open trial");
        breaker.State.Should().Be(EnterpriseSystemCircuitState.HalfOpen);

        // A second concurrent caller must be rejected while the first trial is still in flight.
        breaker.TryEnter().Should().BeFalse("only one trial call may be in flight while half-open");
    }

    [Fact]
    public void RecordSuccess_AfterHalfOpenTrial_ClosesCircuitAndResetsFailureCount()
    {
        // failureThreshold:2 (not 1) so the post-reset assertion below can distinguish "the
        // consecutive-failure count was genuinely reset to zero" from "the circuit never left
        // Closed in the first place".
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 2, openDuration: TimeSpan.Zero);

        breaker.RecordFailure();
        breaker.RecordFailure();
        breaker.State.Should().Be(EnterpriseSystemCircuitState.Open);

        breaker.TryEnter().Should().BeTrue();
        breaker.RecordSuccess();

        breaker.State.Should().Be(EnterpriseSystemCircuitState.Closed);

        // The failure count reset means a single subsequent failure must not immediately re-open
        // a circuit whose threshold is 2.
        breaker.RecordFailure();
        breaker.State.Should().Be(EnterpriseSystemCircuitState.Closed);
    }

    [Fact]
    public void RecordFailure_AfterHalfOpenTrialFails_ReopensCircuit()
    {
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 1, openDuration: TimeSpan.Zero);

        breaker.RecordFailure();
        breaker.TryEnter().Should().BeTrue();
        breaker.RecordFailure();

        breaker.State.Should().Be(EnterpriseSystemCircuitState.Open);
    }

    [Fact]
    public void Constructor_WithNonPositiveFailureThreshold_FallsBackToDefault()
    {
        var breaker = new EnterpriseSystemCircuitBreaker(failureThreshold: 0);

        for (var i = 0; i < EnterpriseSystemCircuitBreaker.DefaultFailureThreshold - 1; i++)
        {
            breaker.RecordFailure();
        }

        breaker.State.Should().Be(EnterpriseSystemCircuitState.Closed, "a non-positive threshold must fall back to the default rather than opening immediately");
    }
}
