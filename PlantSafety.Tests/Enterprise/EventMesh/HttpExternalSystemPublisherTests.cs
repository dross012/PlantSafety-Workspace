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
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Mes, "https://fake-mes.local/inbound", httpClient, CreateEnforcer(), NullLogger<HttpExternalSystemPublisher>());

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        handler.RequestCount.Should().Be(1);
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
}
