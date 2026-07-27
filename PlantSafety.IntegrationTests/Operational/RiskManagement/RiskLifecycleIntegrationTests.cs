// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Operational.RiskManagement
//  Layer:          IntegrationTests
//  File:           RiskLifecycleIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Full Create -> Score -> Prioritize -> ApplyControlMeasure ->
//                  ValidateEffectiveness -> Close -> Archive lifecycle coverage for the Risk
//                  aggregate (Domain.RiskManagement, the "real 3-phase Risk Management workflow"
//                  -- distinct from the Infrastructure.OperationalSafety.RiskManagement read-model
//                  module), exercising the real DI container (IRiskRepository ->
//                  RiskDapperRepository) and the real [RiskMgmt].[Risk] / [RiskMgmt].
//                  [ControlMeasure] tables + usp_Risk_Insert/Update/GetById /
//                  usp_ControlMeasure_Upsert/GetByRiskId stored procedures applied to the
//                  Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyRiskManagementSchemaAsync).
//
//                  Create/Score/Prioritize/ApplyControlMeasure/ValidateEffectiveness/Close are all
//                  sent via MediatR's ISender (mirrors Operations\AuditViewer\
//                  AuditViewerIntegrationTests.cs) since none of the RiskManagement commands
//                  implement IAuthorizedRequest. ASSUMPTION (flagged, per the RiskManagement
//                  module's own README -- RiskManagementApplicationServiceRegistration is a no-op
//                  extension method because handlers/validators are auto-discovered by MediatR's/
//                  FluentValidation's assembly scan, so no composition-root wiring gap exists here):
//                  there is no MediatR command wrapping Risk.Archive() yet (only the domain method
//                  exists), so the final Archive step is exercised directly against the real
//                  IRiskRepository resolved from the DI container, the same repository the (not yet
//                  existent) ArchiveRiskCommand would eventually delegate to.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.Commands;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Operational.RiskManagement;

/// <summary>
/// Exercises the full Risk aggregate lifecycle (Create -&gt; Score -&gt; Prioritize -&gt;
/// ApplyControlMeasure -&gt; ValidateEffectiveness -&gt; Close -&gt; Archive) through the real MediatR
/// pipeline, the real <c>RiskDapperRepository</c>, and the real <c>[RiskMgmt].[Risk]</c> /
/// <c>[RiskMgmt].[ControlMeasure]</c> tables + stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class RiskLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RiskLifecycle_CreateScorePrioritizeApplyControlValidateCloseArchive_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var ownerId = Guid.NewGuid();
            var responsiblePersonId = Guid.NewGuid();

            // ---- Create (hazard identification) --------------------------------
            var createResult = await sender.Send(new CreateRiskCommand(
                CommandId: Guid.NewGuid(),
                CorrelationId: CorrelationId,
                CausationId: Guid.NewGuid(),
                RequestedAtUtc: FixedUtcNow,
                ApiVersionToken: "v1",
                TenantId: TenantId,
                SiteId: Guid.NewGuid(),
                RiskTitle: "Unguarded pinch point on conveyor line 4",
                Location: "Line 4 - Conveyor Assembly",
                HazardSource: "Mechanical",
                OwnerId: ownerId,
                ReviewRequired: true), CancellationToken.None);

            createResult.IsSuccess.Should().BeTrue();
            var riskId = createResult.Value.RiskId;
            createResult.Value.Status.Should().Be("Identified");

            // ---- Score (Severity x Likelihood) ----------------------------------
            var scoreResult = await sender.Send(new ScoreRiskCommand(
                CommandId: Guid.NewGuid(), CorrelationId: CorrelationId, CausationId: Guid.NewGuid(), RequestedAtUtc: FixedUtcNow,
                TenantId: TenantId, RiskId: riskId, Severity: 4, Likelihood: 3, ScoredBy: ownerId), CancellationToken.None);

            scoreResult.IsSuccess.Should().BeTrue();

            // ---- Prioritize -------------------------------------------------------
            var prioritizeResult = await sender.Send(new PrioritizeRiskCommand(
                CommandId: Guid.NewGuid(), CorrelationId: CorrelationId, CausationId: Guid.NewGuid(), RequestedAtUtc: FixedUtcNow,
                TenantId: TenantId, RiskId: riskId, Priority: "P1", PrioritizedBy: ownerId), CancellationToken.None);

            prioritizeResult.IsSuccess.Should().BeTrue();

            // ---- ApplyControlMeasure (Hierarchy of Controls) -----------------------
            var applyControlResult = await sender.Send(new ApplyRiskControlMeasureCommand(
                CommandId: Guid.NewGuid(), CorrelationId: CorrelationId, CausationId: Guid.NewGuid(), RequestedAtUtc: FixedUtcNow,
                TenantId: TenantId, RiskId: riskId, HierarchyLevel: "EngineeringControls",
                Description: "Installed fixed machine guard over pinch point.", Justification: null,
                ResponsiblePersonId: responsiblePersonId), CancellationToken.None);

            applyControlResult.IsSuccess.Should().BeTrue();
            applyControlResult.Value.ControlMeasureId.Should().NotBe(Guid.Empty);

            // ---- ValidateEffectiveness (residual risk) -----------------------------
            var validateResult = await sender.Send(new ValidateRiskEffectivenessCommand(
                CommandId: Guid.NewGuid(), CorrelationId: CorrelationId, CausationId: Guid.NewGuid(), RequestedAtUtc: FixedUtcNow,
                TenantId: TenantId, RiskId: riskId, ResidualSeverity: 2, ResidualLikelihood: 1, ValidatedBy: ownerId), CancellationToken.None);

            validateResult.IsSuccess.Should().BeTrue();
            validateResult.Value.IsAcceptable.Should().BeTrue("residual score 2*1=2 is Low and must be acceptable");
            validateResult.Value.Status.Should().Be("Mitigated");

            // ---- Close ------------------------------------------------------------
            var closeResult = await sender.Send(new CloseRiskCommand(
                CommandId: Guid.NewGuid(), CorrelationId: CorrelationId, CausationId: Guid.NewGuid(), RequestedAtUtc: FixedUtcNow,
                TenantId: TenantId, RiskId: riskId, ClosedBy: ownerId), CancellationToken.None);

            closeResult.IsSuccess.Should().BeTrue();
            closeResult.Value.Status.Should().Be("Closed");

            // ---- Verify persisted state via the real read path before archiving ---
            var repository = scope.ServiceProvider.GetRequiredService<IRiskRepository>();
            var persistedAfterClose = await repository.GetByIdAsync(RiskId.FromGuid(riskId), CancellationToken.None);
            persistedAfterClose.Should().NotBeNull();
            persistedAfterClose!.Status.ToString().Should().Be("Closed");
            persistedAfterClose.ControlMeasures.Should().ContainSingle(cm => cm.Description == "Installed fixed machine guard over pinch point.");

            // ---- Archive (no MediatR command exists yet -- exercised directly against
            // the real repository/domain method, per the class-level ASSUMPTION note) ----
            persistedAfterClose.Archive(
                PlantSafety.SharedKernel.Primitives.ValueTypes.UtcTimestamp.Now(),
                PlantSafety.SharedKernel.Primitives.ValueTypes.PerformedByActor.User(PlantSafety.SharedKernel.Identifiers.UserId.FromGuid(ownerId)),
                new PlantSafety.SharedKernel.Identifiers.CorrelationId(CorrelationId),
                new PlantSafety.SharedKernel.Identifiers.CausationId(Guid.NewGuid()));

            await repository.UpdateAsync(persistedAfterClose, CancellationToken.None);

            var persistedAfterArchive = await repository.GetByIdAsync(RiskId.FromGuid(riskId), CancellationToken.None);
            persistedAfterArchive.Should().NotBeNull();
            persistedAfterArchive!.Status.ToString().Should().Be("Archived", "the real usp_Risk_Update must have persisted the Archived status");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ScoreRisk_ForUnknownRisk_ShouldReturnFailureResult()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new ScoreRiskCommand(
                CommandId: Guid.NewGuid(), CorrelationId: CorrelationId, CausationId: Guid.NewGuid(), RequestedAtUtc: FixedUtcNow,
                TenantId: TenantId, RiskId: Guid.NewGuid(), Severity: 3, Likelihood: 3, ScoredBy: Guid.NewGuid()), CancellationToken.None);

            result.IsSuccess.Should().BeFalse("usp_Risk_GetById returns no row for an unknown RiskId");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
