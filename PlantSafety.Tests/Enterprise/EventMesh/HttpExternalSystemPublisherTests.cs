// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Enterprise.EventMesh
//  Layer:          Tests (Unit)
//  File:           HttpExternalSystemPublisherTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Verifies HttpExternalSystemPublisher's two real behaviours: (1) graceful no-op
//                  (Result.Success(false), no HTTP call) when its endpoint is unconfigured, and
//                  (2) a real HTTP POST against a fake/in-memory HttpMessageHandler (no live ERP/MES
//                  endpoint exists in this environment) when configured, covering both the success
//                  and rejected-status paths. Also covers the Phase 7 §5.3 Data Residency wiring:
//                  outbound publishes for a tenant whose configured DataResidencyPolicy forbids the
//                  platform's current deployment region must be rejected before any HTTP call is made.
//                  Isolated, deterministic, no real network I/O.
// ============================================================================
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Infrastructure.Messaging.EnterpriseMesh;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Tests.Enterprise.EventMesh;

public sealed class HttpExternalSystemPublisherTests : UnitTestBase
{
    private static EnterpriseEventEnvelope SampleEnvelope() =>
        EnterpriseEventEnvelope.Create(
            eventType: "plantsafety.workorder.completed.v1",
            schemaVersion: 1,
            sourceSystem: EnterpriseSourceSystem.Platform,
            occurredAtUtc: FixedUtcNow,
            correlationId: TestCorrelationId.Value,
            causationId: TestCausationId.Value,
            tenantId: TestTenantId.Value,
            actorId: TestUserId.Value,
            payload: new { workOrderId = Guid.NewGuid() });

    /// <summary>Builds a real, unrestricted-by-default <see cref="IDataResidencyEnforcer"/> so existing HTTP-behaviour tests are unaffected.</summary>
    private static IDataResidencyEnforcer CreateEnforcer(
        Region? currentRegion = null, IReadOnlyDictionary<string, string>? tenantPolicies = null)
    {
        var regionProvider = new ConfiguredCurrentDeploymentRegionProvider(
            Options.Create(new CurrentDeploymentRegionOptions { RegionCode = (currentRegion ?? Region.UsEast).Code }));

        var profileStore = new ConfiguredTenantDataResidencyProfileStore(
            Options.Create(new TenantDataResidencyOptions { Policies = tenantPolicies ?? new Dictionary<string, string>() }));

        return new DataResidencyEnforcer(profileStore, regionProvider);
    }

    [Fact]
    public async Task PublishAsync_WithUnconfiguredEndpoint_SkipsGracefullyWithoutCallingHttpClient()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Erp, endpointUrl: string.Empty, httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>());

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse("an unconfigured endpoint must be gracefully skipped, not treated as a failure");
        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishAsync_WithConfiguredEndpointAndSuccessResponse_PublishesAndReturnsTrue()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>());

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        handler.RequestCount.Should().Be(1);
        handler.LastRequestUri.Should().Be("https://fake-erp.local/inbound");
    }

    [Fact]
    public async Task PublishAsync_WithConfiguredEndpointAndRejectedResponse_ReturnsFailure()
    {
        // A 500 is a transient status code and would normally be retried (see the dedicated retry
        // tests below) -- maxAttempts:1 isolates this test to the original "single failed attempt"
        // behaviour so it stays fast and deterministic.
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(
            EnterpriseSourceSystem.Mes, "https://fake-mes.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>(),
            maxAttempts: 1);

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        handler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task PublishAsync_WithNonTransientRejection_DoesNotRetry()
    {
        // 400 Bad Request is a contract/auth problem, not a reachability problem -- must never be
        // retried, even with a multi-attempt budget configured.
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(
            EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>(),
            maxAttempts: 3, retryBaseDelayMs: 1);

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        handler.RequestCount.Should().Be(1, "a non-transient 4xx rejection must never be retried");
    }

    [Fact]
    public async Task PublishAsync_WithTransientFailureThenSuccess_RetriesAndEventuallySucceeds()
    {
        var handler = new FlakyHttpMessageHandler(failuresBeforeSuccess: 2, HttpStatusCode.ServiceUnavailable);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(
            EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>(),
            maxAttempts: 3, retryBaseDelayMs: 1);

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue("the third attempt succeeds, within the configured retry budget");
        handler.RequestCount.Should().Be(3);
    }

    [Fact]
    public async Task PublishAsync_WithPersistentTransientFailure_ExhaustsRetryBudgetAndReturnsFailure()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(
            EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>(),
            maxAttempts: 3, retryBaseDelayMs: 1);

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        handler.RequestCount.Should().Be(3, "every attempt in the retry budget must be consumed before giving up");
    }

    [Fact]
    public async Task PublishAsync_WithTransportException_RetriesWithBackoff()
    {
        var handler = new ThrowingHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(
            EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>(),
            maxAttempts: 3, retryBaseDelayMs: 1);

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        handler.RequestCount.Should().Be(3);
    }

    [Fact]
    public async Task PublishAsync_AfterFailureThresholdReached_OpensCircuitAndShortCircuitsWithoutHttpCall()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(
            EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>(),
            maxAttempts: 1, retryBaseDelayMs: 1, circuitBreakerFailureThreshold: 2, circuitBreakerOpenDurationSeconds: 300);

        // Two failed publishes reach the failure threshold and open the circuit.
        await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);
        await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);
        sut.CircuitState.Should().Be(EnterpriseSystemCircuitState.Open);

        var requestCountBeforeThirdCall = handler.RequestCount;
        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Value.Should().Be("enterprise.eventmesh.circuitopen");
        handler.RequestCount.Should().Be(requestCountBeforeThirdCall, "an open circuit must short-circuit without attempting the HTTP call");
    }

    [Fact]
    public async Task PublishAsync_AfterOpenDurationElapsesAndTrialSucceeds_ClosesCircuit()
    {
        var handler = new FlakyHttpMessageHandler(failuresBeforeSuccess: 2, HttpStatusCode.ServiceUnavailable);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(
            EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>(),
            maxAttempts: 1, retryBaseDelayMs: 1, circuitBreakerFailureThreshold: 2, circuitBreakerOpenDurationSeconds: 0);

        // Two failed publishes (maxAttempts:1 each) reach the threshold and open the circuit;
        // openDurationSeconds:0 means the very next TryEnter() call already admits a half-open trial.
        await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);
        await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);
        sut.CircuitState.Should().Be(EnterpriseSystemCircuitState.Open);

        // The flaky handler's third call (the half-open trial) succeeds.
        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sut.CircuitState.Should().Be(EnterpriseSystemCircuitState.Closed);
    }

    [Fact]
    public async Task PublishAsync_TenantResidencyPolicyViolatedByCurrentRegion_RejectsWithoutCallingHttpClient()
    {
        var envelope = SampleEnvelope();
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var enforcer = CreateEnforcer(
            currentRegion: Region.UsEast,
            tenantPolicies: new Dictionary<string, string> { [envelope.TenantId.ToString("D")] = "EuOnly" });
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, enforcer, NullLogger<HttpExternalSystemPublisher>());

        var result = await sut.PublishAsync(envelope, CancellationToken.None);

        result.IsFailure.Should().BeTrue("an EU-only tenant's data must never be published from a US deployment region");
        result.Error!.Code.Value.Should().Be("enterprise.eventmesh.residencyviolation");
        handler.RequestCount.Should().Be(0, "the HTTP call must never be attempted once the residency check rejects the publish");
    }

    [Fact]
    public async Task PublishAsync_TenantResidencyPolicySatisfiedByCurrentRegion_ProceedsToHttpCall()
    {
        var envelope = SampleEnvelope();
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var enforcer = CreateEnforcer(
            currentRegion: Region.UsEast,
            tenantPolicies: new Dictionary<string, string> { [envelope.TenantId.ToString("D")] = "UsOnly" });
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, enforcer, NullLogger<HttpExternalSystemPublisher>());

        var result = await sut.PublishAsync(envelope, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        handler.RequestCount.Should().Be(1);
    }

    /// <summary>Fake, in-memory HttpMessageHandler standing in for the (nonexistent-in-this-sandbox) external ERP/MES endpoint.</summary>
    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public string? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequestUri = request.RequestUri?.ToString();
            return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent("{}") });
        }
    }

    /// <summary>Fake HttpMessageHandler that fails with <paramref name="failureStatusCode"/> for the first <paramref name="failuresBeforeSuccess"/> requests, then returns 200 OK.</summary>
    private sealed class FlakyHttpMessageHandler(int failuresBeforeSuccess, HttpStatusCode failureStatusCode) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var statusCode = RequestCount <= failuresBeforeSuccess ? failureStatusCode : HttpStatusCode.OK;
            return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent("{}") });
        }
    }

    /// <summary>Fake HttpMessageHandler that always throws <see cref="HttpRequestException"/>, simulating connection-refused/DNS-failure transport errors.</summary>
    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            throw new HttpRequestException("Simulated connection-refused failure.");
        }
    }
}
