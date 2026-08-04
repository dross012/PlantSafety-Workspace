// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh
//  Layer:          Infrastructure
//  File:           EnterpriseSystemCircuitBreaker.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 9 §1 Reliability Hardening, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Minimal, hand-rolled circuit-breaker state machine (Closed / Open / HalfOpen)
//                  guarding one HttpExternalSystemPublisher's per-endpoint outbound calls (ERP/MES/
//                  CMMS/HR/Vendor each have their own configured endpoint per
//                  EnterpriseEventMeshOptions, and each gets its own HttpExternalSystemPublisher
//                  singleton instance -- see InfrastructureServiceRegistration -- so one circuit
//                  breaker instance per target system is the correct scope). No third-party
//                  resilience library (Polly, Microsoft.Extensions.Http.Resilience) is referenced
//                  anywhere in this codebase's .csproj files as of this pass, so this is a small,
//                  genuinely testable, dependency-free state machine rather than a new package this
//                  late in hardening. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

/// <summary>The three states a <see cref="EnterpriseSystemCircuitBreaker"/> can be in.</summary>
public enum EnterpriseSystemCircuitState
{
    /// <summary>Calls are permitted; failures are being counted toward <c>FailureThreshold</c>.</summary>
    Closed,

    /// <summary>Calls are rejected without attempting the underlying transport until the open duration elapses.</summary>
    Open,

    /// <summary>Exactly one trial call is permitted to probe whether the endpoint has recovered.</summary>
    HalfOpen
}

/// <summary>
/// Thread-safe, in-process circuit-breaker state machine: after
/// <see cref="FailureThreshold"/> consecutive failures the circuit opens and every call is
/// short-circuited (rejected without attempting the transport) for <see cref="OpenDuration"/>. Once
/// that duration elapses, the circuit moves to <see cref="EnterpriseSystemCircuitState.HalfOpen"/>
/// and admits exactly one trial call: success closes the circuit and resets the failure count;
/// failure re-opens it. Callers drive the state machine via <see cref="TryEnter"/> (call before
/// attempting the transport) and <see cref="RecordSuccess"/>/<see cref="RecordFailure"/> (call after
/// the attempt resolves).
/// </summary>
public sealed class EnterpriseSystemCircuitBreaker
{
    /// <summary>Default number of consecutive failures before the circuit opens.</summary>
    public const int DefaultFailureThreshold = 5;

    /// <summary>Default duration, in seconds, the circuit stays open before admitting a half-open trial call.</summary>
    public const int DefaultOpenDurationSeconds = 30;

    private readonly int _failureThreshold;
    private readonly TimeSpan _openDuration;
    private readonly object _gate = new();

    private EnterpriseSystemCircuitState _state = EnterpriseSystemCircuitState.Closed;
    private int _consecutiveFailures;
    private DateTime _openedAtUtc;
    private bool _halfOpenTrialInFlight;

    /// <summary>Initializes a new instance of <see cref="EnterpriseSystemCircuitBreaker"/>.</summary>
    /// <param name="failureThreshold">Consecutive failures before the circuit opens. Non-positive values fall back to <see cref="DefaultFailureThreshold"/>.</param>
    /// <param name="openDuration">How long the circuit stays open before a half-open trial is admitted. Defaults to <see cref="DefaultOpenDurationSeconds"/> seconds.</param>
    public EnterpriseSystemCircuitBreaker(int failureThreshold = DefaultFailureThreshold, TimeSpan? openDuration = null)
    {
        _failureThreshold = failureThreshold > 0 ? failureThreshold : DefaultFailureThreshold;
        _openDuration = openDuration ?? TimeSpan.FromSeconds(DefaultOpenDurationSeconds);
    }

    /// <summary>The circuit's current state.</summary>
    public EnterpriseSystemCircuitState State
    {
        get { lock (_gate) return _state; }
    }

    /// <summary>
    /// Determines whether the caller may proceed with the underlying call. Transitions
    /// <see cref="EnterpriseSystemCircuitState.Open"/> to <see cref="EnterpriseSystemCircuitState.HalfOpen"/>
    /// once <see cref="OpenDuration"/> (captured at construction) has elapsed, admitting exactly one
    /// trial call; concurrent callers while a half-open trial is already in flight are rejected until
    /// that trial resolves via <see cref="RecordSuccess"/> or <see cref="RecordFailure"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the call may proceed; <see langword="false"/> if it must be short-circuited.</returns>
    public bool TryEnter()
    {
        lock (_gate)
        {
            switch (_state)
            {
                case EnterpriseSystemCircuitState.Closed:
                    return true;

                case EnterpriseSystemCircuitState.Open:
                    if (DateTime.UtcNow - _openedAtUtc < _openDuration)
                    {
                        return false;
                    }

                    _state = EnterpriseSystemCircuitState.HalfOpen;
                    _halfOpenTrialInFlight = true;
                    return true;

                case EnterpriseSystemCircuitState.HalfOpen:
                    if (_halfOpenTrialInFlight)
                    {
                        return false;
                    }

                    _halfOpenTrialInFlight = true;
                    return true;

                default:
                    return true;
            }
        }
    }

    /// <summary>Records a successful call: closes the circuit and resets the consecutive-failure count.</summary>
    public void RecordSuccess()
    {
        lock (_gate)
        {
            _consecutiveFailures = 0;
            _state = EnterpriseSystemCircuitState.Closed;
            _halfOpenTrialInFlight = false;
        }
    }

    /// <summary>
    /// Records a failed call. A failed half-open trial immediately re-opens the circuit; a failed
    /// call while closed increments the consecutive-failure count and opens the circuit once
    /// <see cref="_failureThreshold"/> is reached.
    /// </summary>
    public void RecordFailure()
    {
        lock (_gate)
        {
            _halfOpenTrialInFlight = false;

            if (_state == EnterpriseSystemCircuitState.HalfOpen)
            {
                _state = EnterpriseSystemCircuitState.Open;
                _openedAtUtc = DateTime.UtcNow;
                return;
            }

            _consecutiveFailures++;
            if (_consecutiveFailures >= _failureThreshold)
            {
                _state = EnterpriseSystemCircuitState.Open;
                _openedAtUtc = DateTime.UtcNow;
            }
        }
    }
}
