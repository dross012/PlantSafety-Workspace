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
//                  and rejected-status paths. Isolated, deterministic, no real network I/O.
// ============================================================================
using System.Net;
using System.Text.Json;
using PlantSafety.Infrastructure.Messaging.EnterpriseMesh;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

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

    [Fact]
    public async Task PublishAsync_WithUnconfiguredEndpoint_SkipsGracefullyWithoutCallingHttpClient()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Erp, endpointUrl: string.Empty, httpClient, NullLogger<HttpExternalSystemPublisher>());

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
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Erp, "https://fake-erp.local/inbound", httpClient, NullLogger<HttpExternalSystemPublisher>());

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
        var sut = new HttpExternalSystemPublisher(EnterpriseSourceSystem.Mes, "https://fake-mes.local/inbound", httpClient, NullLogger<HttpExternalSystemPublisher>());

        var result = await sut.PublishAsync(SampleEnvelope(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
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
