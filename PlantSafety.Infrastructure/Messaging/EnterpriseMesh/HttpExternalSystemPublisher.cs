// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh
//  Layer:          Infrastructure
//  File:           HttpExternalSystemPublisher.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Real, working HTTP-based IExternalSystemPublisher: POSTs the
//                  EnterpriseEventEnvelope as JSON to a configurable endpoint. Gracefully no-ops
//                  (logs at Debug, returns a successful "skipped" Result) when its configured
//                  endpoint is empty -- matching this codebase's existing "optional, gracefully
//                  degrading" integration convention (see BlobStorageOptions/AzureBlobStorageService
//                  defaulting to the Azurite emulator connection string when unset). No live
//                  ERP/MES credentials exist in this sandboxed environment; integration tests
//                  exercise this class against an in-memory/fake HTTP handler (see
//                  Shared.EventBus.Tests-style precedent) rather than a live endpoint. Additive-only
//                  evolution.
//
//                  2026-08-03 UPDATE (Phase 7 §5.3 Data Residency wiring): publishing an envelope to
//                  an external ERP/MES/CMMS system is a genuinely cross-region-relevant operation --
//                  the external endpoint may sit outside the tenant's required residency zone. Before
//                  any HTTP transmission this now calls IDataResidencyEnforcer.IsSatisfied(tenantId)
//                  (reusing the same tenant-vs-current-deployment-region comparison the enforcer
//                  already performs for tenant data exports) and rejects the publish -- loudly logged,
//                  never silently dropped -- when the tenant's policy does not permit it. Additive-only
//                  evolution.
//
//                  2026-08-03 UPDATE (Phase 9 §1 Reliability Hardening): this class previously made
//                  exactly one HTTP attempt with no retry of any kind -- a single transient timeout,
//                  5xx, or connection-refused permanently failed the publish. No Polly/
//                  Microsoft.Extensions.Http.Resilience dependency exists anywhere in this codebase's
//                  .csproj files, so this adds a small, dependency-free, bounded exponential-backoff
//                  retry loop (reusing InfrastructureConstants.DefaultRetryCount/RetryBaseDelayMs --
//                  the same constants cfg.UseMessageRetry already uses for the internal MassTransit
//                  bus) around transient failures only (5xx, 408 Request Timeout,
//                  HttpRequestException, TaskCanceledException) -- a non-transient 4xx rejection
//                  (e.g. 400/401/403/422, a genuine contract/auth problem, not a reachability
//                  problem) is never retried. Also adds a real, small, testable
//                  EnterpriseSystemCircuitBreaker (Closed/Open/HalfOpen) per endpoint: once an
//                  endpoint has failed persistently, the circuit opens and further calls are
//                  short-circuited (no HTTP attempt at all) for a bounded open duration, so a
//                  down external system stops being hammered by every subsequent outbound event.
//                  Only transient/transport failures (and their retry exhaustion) count toward the
//                  circuit breaker -- a residency rejection or a non-transient 4xx is a business
//                  decision, not evidence the endpoint is unreachable, and must not trip the breaker.
//                  New constructor parameters are optional/defaulted so every existing call site
//                  (InfrastructureServiceRegistration, unit tests) compiles unchanged.
//                  Additive-only evolution.
//
//                  2026-08-03 UPDATE (Phase 9 §4 Observability & Telemetry Expansion): outbound
//                  ERP/MES/CMMS/HR/Vendor call latency was previously only ever a log line (LogInformation/
//                  LogWarning), with no queryable record of how long any given publish actually took --
//                  a genuine observability gap distinct from the already-real
//                  PlatformHealthDashboard inbound-HTTP-request metrics. Rather than build a parallel
//                  metrics table, this reuses IRequestMetricsRecorder/[PlatformHealth].[RequestMetricEvent]
//                  additively: its (method, path, statusCode, elapsedMs, correlationId, tenantId) shape
//                  genuinely fits an outbound call just as well as an inbound one, so one sample is
//                  recorded per PublishAsync call that actually attempts the network (Method=
//                  "OUTBOUND:{TargetSystem}", Path=endpointUrl, StatusCode=the final HTTP status or a
//                  synthetic negative code for transport-level failure). The three early-return paths
//                  that never touch the network (endpoint not configured, residency rejection, circuit
//                  breaker open) are deliberately NOT recorded here -- they measure zero real latency
//                  and are already independently observable (log line + CircuitState property).
//                  Optional/defaulted constructor parameter so every existing call site compiles
//                  unchanged. Additive-only evolution.
// ============================================================================
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using PlantSafety.Application.Abstractions.Observability;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Infrastructure.Common.Constants;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

/// <summary>HTTP-based <see cref="IExternalSystemPublisher"/> for one external system's Enterprise Event Mesh endpoint.</summary>
public sealed class HttpExternalSystemPublisher(
    EnterpriseSourceSystem targetSystem,
    string endpointUrl,
    HttpClient httpClient,
    IDataResidencyEnforcer dataResidencyEnforcer,
    ILogger<HttpExternalSystemPublisher> logger,
    int maxAttempts = InfrastructureConstants.DefaultRetryCount,
    int retryBaseDelayMs = InfrastructureConstants.RetryBaseDelayMs,
    int circuitBreakerFailureThreshold = EnterpriseSystemCircuitBreaker.DefaultFailureThreshold,
    int circuitBreakerOpenDurationSeconds = EnterpriseSystemCircuitBreaker.DefaultOpenDurationSeconds,
    IRequestMetricsRecorder? metricsRecorder = null) : IExternalSystemPublisher
{
    private readonly int _maxAttempts = maxAttempts > 0 ? maxAttempts : InfrastructureConstants.DefaultRetryCount;
    private readonly int _retryBaseDelayMs = retryBaseDelayMs >= 0 ? retryBaseDelayMs : InfrastructureConstants.RetryBaseDelayMs;

    private readonly EnterpriseSystemCircuitBreaker _circuitBreaker =
        new(circuitBreakerFailureThreshold, TimeSpan.FromSeconds(circuitBreakerOpenDurationSeconds));

    /// <inheritdoc/>
    public EnterpriseSourceSystem TargetSystem { get; } = targetSystem;

    /// <summary>The endpoint's current circuit-breaker state, exposed for diagnostics/health-check surfaces.</summary>
    public EnterpriseSystemCircuitState CircuitState => _circuitBreaker.State;

    /// <inheritdoc/>
    public async Task<Result<bool>> PublishAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            logger.LogDebug(
                "EnterpriseEventMesh: {TargetSystem} endpoint is not configured; skipping outbound publish of {EventType} (EventId={EventId}).",
                TargetSystem, envelope.EventType, envelope.EventId);
            return Result<bool>.Success(false);
        }

        if (envelope.TenantId != Guid.Empty && !dataResidencyEnforcer.IsSatisfied(envelope.TenantId))
        {
            logger.LogWarning(
                "EnterpriseEventMesh: rejected outbound publish of {EventType} (EventId={EventId}) for Tenant {TenantId} to " +
                "{TargetSystem} -- the tenant's configured data-residency policy does not permit sending data outside the " +
                "platform's current deployment region.",
                envelope.EventType, envelope.EventId, envelope.TenantId, TargetSystem);

            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.residencyviolation"),
                ErrorType.Forbidden,
                $"Tenant {envelope.TenantId}'s data-residency policy does not permit publishing to {TargetSystem} from the platform's current deployment region."));
        }

        if (!_circuitBreaker.TryEnter())
        {
            logger.LogWarning(
                "EnterpriseEventMesh: circuit breaker OPEN for {TargetSystem}; short-circuiting publish of {EventType} " +
                "(EventId={EventId}) without attempting the HTTP call.",
                TargetSystem, envelope.EventType, envelope.EventId);

            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.circuitopen"),
                ErrorType.DependencyUnavailable,
                $"{TargetSystem} endpoint's circuit breaker is open -- the endpoint has failed persistently and is being given time to recover."));
        }

        var stopwatch = Stopwatch.StartNew();
        var result = await PublishWithRetryAsync(envelope, cancellationToken);
        stopwatch.Stop();

        // Best-effort, non-blocking, never-throwing (see IRequestMetricsRecorder contract) --
        // records one sample per network-attempting publish so outbound integration latency is
        // queryable via [PlatformHealth].[RequestMetricEvent], not just a log line.
        metricsRecorder?.Record(
            method: $"OUTBOUND:{TargetSystem}",
            path: endpointUrl,
            statusCode: ResolveMetricStatusCode(result),
            elapsedMs: stopwatch.ElapsedMilliseconds,
            correlationId: envelope.CorrelationId,
            tenantId: envelope.TenantId);

        return result;
    }

    /// <summary>
    /// Maps a publish <see cref="Result{T}"/> to a synthetic HTTP-style status code for the
    /// shared RequestMetricEvent shape: 200 on success, 502 for any recorded transport/rejection
    /// failure (the specific status is already captured in the per-attempt log line -- this column
    /// exists for coarse success/failure-rate aggregation, not fine-grained status breakdown).
    /// </summary>
    private static int ResolveMetricStatusCode(Result<bool> result) => result.IsSuccess ? 200 : 502;

    /// <summary>
    /// Attempts the HTTP publish up to <see cref="_maxAttempts"/> times, retrying only transient
    /// failures (5xx, 408, HttpRequestException, TaskCanceledException) with exponential backoff
    /// (<see cref="_retryBaseDelayMs"/> * 2^(attempt-1)), then records the final outcome on the
    /// circuit breaker.
    /// </summary>
    private async Task<Result<bool>> PublishWithRetryAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(endpointUrl, envelope, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation(
                        "EnterpriseEventMesh: published {EventType} (EventId={EventId}) to {TargetSystem} on attempt {Attempt}/{MaxAttempts}.",
                        envelope.EventType, envelope.EventId, TargetSystem, attempt, _maxAttempts);

                    _circuitBreaker.RecordSuccess();
                    return Result<bool>.Success(true);
                }

                if (!IsTransientStatusCode(response.StatusCode) || attempt == _maxAttempts)
                {
                    logger.LogWarning(
                        "EnterpriseEventMesh: {TargetSystem} rejected {EventType} (EventId={EventId}) with status {StatusCode} " +
                        "on attempt {Attempt}/{MaxAttempts} ({Transience}).",
                        TargetSystem, envelope.EventType, envelope.EventId, (int)response.StatusCode, attempt, _maxAttempts,
                        IsTransientStatusCode(response.StatusCode) ? "retry budget exhausted" : "non-transient, not retried");

                    // A non-transient 4xx rejection (bad contract/auth, not endpoint unavailability)
                    // must not trip the circuit breaker -- only transient/transport exhaustion does.
                    if (IsTransientStatusCode(response.StatusCode))
                    {
                        _circuitBreaker.RecordFailure();
                    }

                    return Result<bool>.Failure(new Error(
                        ErrorCode.Create("enterprise.eventmesh.publishrejected"),
                        ErrorType.DependencyFailure,
                        $"{TargetSystem} endpoint rejected the event with status {(int)response.StatusCode} after {attempt} attempt(s)."));
                }

                logger.LogWarning(
                    "EnterpriseEventMesh: {TargetSystem} returned transient status {StatusCode} for {EventType} " +
                    "(EventId={EventId}) on attempt {Attempt}/{MaxAttempts}; retrying with backoff.",
                    TargetSystem, (int)response.StatusCode, envelope.EventType, envelope.EventId, attempt, _maxAttempts);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                if (attempt == _maxAttempts)
                {
                    logger.LogWarning(ex,
                        "EnterpriseEventMesh: transport failure publishing {EventType} (EventId={EventId}) to {TargetSystem} " +
                        "after {Attempt}/{MaxAttempts} attempts; retry budget exhausted.",
                        envelope.EventType, envelope.EventId, TargetSystem, attempt, _maxAttempts);

                    _circuitBreaker.RecordFailure();

                    return Result<bool>.Failure(new Error(
                        ErrorCode.Create("enterprise.eventmesh.publishtransportfailure"),
                        ErrorType.DependencyUnavailable,
                        $"Transport failure while publishing to {TargetSystem} after {attempt} attempt(s)."));
                }

                logger.LogWarning(ex,
                    "EnterpriseEventMesh: transient transport failure publishing {EventType} (EventId={EventId}) to " +
                    "{TargetSystem} on attempt {Attempt}/{MaxAttempts}; retrying with backoff.",
                    envelope.EventType, envelope.EventId, TargetSystem, attempt, _maxAttempts);
            }

            var delayMs = _retryBaseDelayMs * (1 << (attempt - 1));
            await Task.Delay(delayMs, cancellationToken);
        }

        // Unreachable: the loop above always returns on its final iteration. Present for the
        // compiler's benefit and to fail loudly rather than silently if that invariant is ever broken.
        throw new InvalidOperationException($"{nameof(PublishWithRetryAsync)} exited its retry loop without returning a result.");
    }

    /// <summary>Transient HTTP status codes worth retrying: 5xx server errors and 408 Request Timeout.</summary>
    private static bool IsTransientStatusCode(HttpStatusCode statusCode) =>
        (int)statusCode >= 500 || statusCode == HttpStatusCode.RequestTimeout;
}
