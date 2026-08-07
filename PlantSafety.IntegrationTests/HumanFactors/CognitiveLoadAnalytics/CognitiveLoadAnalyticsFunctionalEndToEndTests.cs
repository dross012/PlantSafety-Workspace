// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HumanFactors.CognitiveLoadAnalytics
//  Layer:          IntegrationTests
//  File:           CognitiveLoadAnalyticsFunctionalEndToEndTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Functional / End-to-End / Scenario coverage for the
//                   HumanFactors.CognitiveLoadAnalytics sub-module. Exercises the full command
//                   pipeline (MediatR ISender -> handler -> real Dapper repository -> real
//                   [HumanFactorsCognitiveLoadAnalytics] tables/procs on the Testcontainers SQL
//                   Server instance) for the complete worker-shift scenario: record readings
//                   (including an overload reading) -> define a task-complexity profile ->
//                   recalculate the workload trend -> generate a predictive cognitive-risk
//                   assessment -> acknowledge the resulting alert -> verify all 4 queries surface
//                   the persisted state. Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.IntegrationTests.HumanFactors.CognitiveLoadAnalytics;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CognitiveLoadAnalyticsFunctionalEndToEndTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task FullShiftScenario_RecordReadingsDefineProfileRecalculateTrendGeneratePredictionAcknowledge_RoundTripsThroughRealStoredProcedures()
    {
        // GetScopedService (not a plain CreateScope) is required here: it populates a fake
        // authenticated HttpContext with the fixed TestIds tenant so TenantIsolationBehavior
        // resolves a tenant context to compare the command's TenantId against -- without it,
        // every command sent through this sender is rejected with
        // ApplicationAuthorizationException("The request's TenantId does not match the
        // authenticated tenant context.").
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        using var _ = scope;

        var workerId = Guid.NewGuid();
        var shiftDateUtc = DateTime.UtcNow.Date;

        // ---- Record a low-load reading (opens the shift's ledger) --------------------------
        var firstReading = RecordCognitiveLoadReadingCommand.FromDto(
            tenantId: TenantId, siteId: null, shiftType: "Day", shiftDateUtc: shiftDateUtc,
            workerUserId: workerId, taskComplexityProfileId: null, cognitiveLoadScore: 40m,
            measurementMethod: "SelfReport", measuredAtUtc: DateTime.UtcNow.AddHours(-2), requesterUserId: ActorId);
        var firstResult = await sender.Send(firstReading, CancellationToken.None);
        firstResult.IsSuccess.Should().BeTrue();
        firstResult.Value.IsOverload.Should().BeFalse();
        var ledgerId = firstResult.Value.CognitiveLoadLedgerId;

        // ---- Record an overload reading on the same (now-existing) ledger -------------------
        var overloadReading = RecordCognitiveLoadReadingCommand.FromDto(
            tenantId: TenantId, siteId: null, shiftType: "Day", shiftDateUtc: shiftDateUtc,
            workerUserId: workerId, taskComplexityProfileId: null, cognitiveLoadScore: 92m,
            measurementMethod: "NASA-TLX", measuredAtUtc: DateTime.UtcNow.AddHours(-1), requesterUserId: ActorId);
        var overloadResult = await sender.Send(overloadReading, CancellationToken.None);
        overloadResult.IsSuccess.Should().BeTrue();
        overloadResult.Value.IsOverload.Should().BeTrue();
        overloadResult.Value.CognitiveLoadLedgerId.Should().Be(ledgerId, "the second reading must land on the same open shift ledger");

        // ---- Define a task-complexity profile on the same ledger ---------------------------
        var taskCode = $"CTRL-{Guid.NewGuid():N}"[..20];
        var defineProfile = DefineTaskComplexityProfileCommand.FromDto(
            tenantId: TenantId, siteId: null, shiftType: "Day", shiftDateUtc: shiftDateUtc,
            taskCode: taskCode, taskName: "Control Room Startup", complexityScore: 7,
            description: "High-stakes multi-step startup sequence.", requesterUserId: ActorId);
        var profileResult = await sender.Send(defineProfile, CancellationToken.None);
        profileResult.IsSuccess.Should().BeTrue();
        profileResult.Value.CognitiveLoadLedgerId.Should().Be(ledgerId);

        // ---- Recalculate the workload trend across both readings ---------------------------
        var recalc = RecalculateWorkloadTrendCommand.FromDto(
            tenantId: TenantId, cognitiveLoadLedgerId: ledgerId,
            periodStartUtc: DateTime.UtcNow.AddHours(-3), periodEndUtc: DateTime.UtcNow, requesterUserId: ActorId);
        var recalcResult = await sender.Send(recalc, CancellationToken.None);
        recalcResult.IsSuccess.Should().BeTrue();
        recalcResult.Value.ReadingCount.Should().Be(2);
        recalcResult.Value.OverloadEventCount.Should().Be(1);
        recalcResult.Value.MeanCognitiveLoadScore.Should().Be(66m);

        // ---- Generate a predictive cognitive-risk assessment from the ledger ----------------
        var generate = GenerateCognitiveRiskPredictionCommand.FromDto(
            tenantId: TenantId, siteId: null, workerUserId: workerId, sourceLedgerId: ledgerId,
            modelVersion: "v1.0.0-integration-test", requesterUserId: ActorId);
        var generateResult = await sender.Send(generate, CancellationToken.None);
        generateResult.IsSuccess.Should().BeTrue();
        var predictionId = generateResult.Value.CognitiveRiskPredictionId;

        // ---- Acknowledge the resulting alert -------------------------------------------------
        var acknowledge = AcknowledgeCognitiveRiskAlertCommand.FromDto(
            tenantId: TenantId, cognitiveRiskPredictionId: predictionId,
            acknowledgementNotes: "Reviewed with worker; coaching scheduled.", requesterUserId: ActorId);
        var acknowledgeResult = await sender.Send(acknowledge, CancellationToken.None);
        acknowledgeResult.IsSuccess.Should().BeTrue();
        acknowledgeResult.Value.Status.Should().Be("Acknowledged");

        // ---- Verify all 4 queries surface the persisted state --------------------------------
        var byWorker = await sender.Send(
            new GetCognitiveLoadByWorkerQuery(TenantId, workerId, DateTime.UtcNow.AddHours(-3), DateTime.UtcNow), CancellationToken.None);
        byWorker.IsSuccess.Should().BeTrue();
        byWorker.Value.ReadingCount.Should().Be(2);
        byWorker.Value.OverloadEventCount.Should().Be(1);

        var profile = await sender.Send(new GetTaskComplexityProfileQuery(TenantId, taskCode), CancellationToken.None);
        profile.IsSuccess.Should().BeTrue();
        profile.Value.CognitiveLoadLedgerId.Should().Be(ledgerId);

        var trend = await sender.Send(
            new GetWorkloadTrendByShiftQuery(TenantId, null, "Day", DateTime.UtcNow.AddHours(-3), DateTime.UtcNow), CancellationToken.None);
        trend.IsSuccess.Should().BeTrue();
        trend.Value.Should().Contain(t => t.CognitiveLoadLedgerId == ledgerId && t.ReadingCount == 2);

        var elevated = await sender.Send(new ListElevatedCognitiveRiskPredictionsQuery(TenantId, null), CancellationToken.None);
        elevated.IsSuccess.Should().BeTrue();
        // The acknowledged prediction is Elevated/Critical-eligible but not Dismissed, so it must
        // still surface on the board even after acknowledgement (only Dismissed is excluded).
        elevated.Value.Should().Contain(p => p.CognitiveRiskPredictionId == predictionId);
    }
}
