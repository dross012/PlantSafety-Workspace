// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.AccidentManagement
//  Layer:          IntegrationTests
//  File:           CorrectiveActionLifecycleIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Full corrective-action (CAPA) lifecycle coverage -- Create Accident -> assign
//                  investigator -> record root cause -> CreateCorrectiveAction -> Complete ->
//                  VerifyEffective -> CloseInvestigation -> Archive -- exercising the real DI
//                  container (IAccidentRepository -> AccidentDapperRepository) and the real
//                  [AccidentManagement].[Accident] / [AccidentManagement].[CorrectiveAction] tables
//                  + usp_Accident_Insert/Update/GetById and usp_CorrectiveAction_Upsert/
//                  GetByAccidentId stored procedures applied to the Testcontainers SQL Server
//                  instance (see Setup\TestDatabaseFactory.ApplyAccidentManagementSchemaAsync).
//
//                  ASSUMPTION (flagged, ambiguity noted in the task brief): PlantSafety has two
//                  distinct "corrective action" concepts. (1) Features\ContinuousImprovement\CAPA's
//                  CreateCorrectiveActionCommand / ICorrectiveActionRepository /
//                  CorrectiveActionDapperRepository, which calls
//                  [ContinuousImprovement].[usp_CorrectiveAction_Create]/[usp_CorrectiveAction_
//                  GetById]/[usp_CorrectiveAction_List] against an implied
//                  [ContinuousImprovement].[CorrectiveAction] table that has NO tracked CREATE
//                  TABLE/CREATE PROCEDURE script anywhere in the repository (confirmed via search;
//                  same "pre-existing, unversioned" situation Shared.SafetyData.Abstractions.
//                  Models.CorrectiveActionEntry's own header comment already documents). (2)
//                  AccidentManagement.CorrectiveAction -- a child entity of the Accident aggregate
//                  (Domain.AccidentManagement.Entities.CorrectiveAction /
//                  Domain.AccidentManagement.Aggregates.Accident.CreateCorrectiveAction), backed by
//                  a real, version-controlled table + stored procedures
//                  (PlantSafety.Infrastructure\AccidentManagement\Sql\Tables\CorrectiveAction.sql,
//                  \Procedures\usp_CorrectiveAction_Upsert.sql /
//                  usp_CorrectiveAction_GetByAccidentId.sql). Per this agent's mandate to ground
//                  every fixture in real, verifiable schema (never invent column names/procedures),
//                  this test exercises path (2) -- the AccidentManagement.CorrectiveAction
//                  lifecycle. There is no MediatR command wrapping the Accident aggregate's
//                  lifecycle methods yet, so they are invoked directly against the real
//                  IAccidentRepository resolved from the DI container.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.IntegrationTests.AccidentManagement;

/// <summary>
/// Exercises the full CorrectiveAction (CAPA) lifecycle -- Create -&gt; Complete -&gt;
/// VerifyEffective -- as a child entity of the Accident aggregate, through the real
/// <c>AccidentDapperRepository</c> and the real <c>[AccidentManagement].[Accident]</c> /
/// <c>[AccidentManagement].[CorrectiveAction]</c> tables + stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CorrectiveActionLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CorrectiveActionLifecycle_CreateCompleteVerifyThroughAccidentAggregate_ShouldRoundTripThroughRealStoredProcedures()
    {
        var scope = Factory.Services.CreateScope();
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccidentRepository>();

            var ownerId = UserId.FromGuid(Guid.NewGuid());
            var investigatorId = UserId.FromGuid(Guid.NewGuid());
            var responsiblePersonId = UserId.FromGuid(Guid.NewGuid());
            var correlationId = new CorrelationId(CorrelationId);
            var actor = PerformedByActor.User(ownerId);

            // ---- Create Accident (Reported) -------------------------------------
            var accident = Accident.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: correlationId,
                causationId: new CausationId(Guid.NewGuid()),
                actor: actor,
                tenantId: TenantId,
                siteId: Guid.NewGuid(),
                accidentTitle: "Slip on wet floor near loading dock",
                location: "Loading Dock B",
                severity: AccidentSeverity.FirstAidOnly,
                injuredPersonCount: 1,
                occurredAtUtc: UtcTimestamp.Now(),
                ownerId: ownerId,
                reviewRequired: true);

            await repository.AddAsync(accident, CancellationToken.None);

            // ---- Progress the investigation to a state that allows CAPA creation ---
            accident.ConfirmSiteSecuredAndTriaged();
            accident.AssignInvestigator(investigatorId, UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            accident.RecordRootCause(
                RootCauseCategory.Environmental,
                "Wet floor was not marked with a warning sign after mopping.",
                UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));

            var correctiveAction = accident.CreateCorrectiveAction(
                CorrectiveActionCategory.AdministrativeControls,
                "Install 'Wet Floor' signage protocol and daily mopping schedule audit.",
                responsiblePersonId,
                UtcTimestamp.From(DateTime.UtcNow.AddDays(14)),
                UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));

            await repository.UpdateAsync(accident, CancellationToken.None);

            // ---- GetById -- verify the CAPA child row persisted via the real upsert proc
            var afterCreate = await repository.GetByIdAsync(accident.AccidentId, TenantId, CancellationToken.None);
            afterCreate.Should().NotBeNull();
            afterCreate!.Status.ToString().Should().Be("CorrectiveActionsInProgress");
            afterCreate.CorrectiveActions.Should().ContainSingle();
            var persistedAction = afterCreate.CorrectiveActions.Single();
            persistedAction.CorrectiveActionId.Should().Be(correctiveAction.CorrectiveActionId);
            persistedAction.Status.ToString().Should().Be("Open");
            persistedAction.Category.Code.Should().Be("AdministrativeControls");

            // ---- Complete the corrective action -----------------------------------
            afterCreate.CompleteCorrectiveAction(
                correctiveAction.CorrectiveActionId,
                "Signage installed 2026-07-27; audit checklist added to shift handover.",
                UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));

            await repository.UpdateAsync(afterCreate, CancellationToken.None);

            var afterComplete = await repository.GetByIdAsync(accident.AccidentId, TenantId, CancellationToken.None);
            afterComplete.Should().NotBeNull();
            var completedAction = afterComplete!.CorrectiveActions.Single(a => a.CorrectiveActionId == correctiveAction.CorrectiveActionId);
            completedAction.Status.ToString().Should().Be("Completed");
            completedAction.CompletionEvidence.Should().Contain("Signage installed");

            // ---- VerifyEffective (Full Risk Management Process, Step 7) -----------
            completedAction.VerifyEffective(ownerId, "Follow-up walkthrough confirmed signage present and audit log up to date.", UtcTimestamp.Now());
            await repository.UpdateAsync(afterComplete, CancellationToken.None);

            var afterVerify = await repository.GetByIdAsync(accident.AccidentId, TenantId, CancellationToken.None);
            afterVerify.Should().NotBeNull();
            var verifiedAction = afterVerify!.CorrectiveActions.Single(a => a.CorrectiveActionId == correctiveAction.CorrectiveActionId);
            verifiedAction.Status.ToString().Should().Be("VerifiedEffective");
            verifiedAction.VerifiedBy.Should().Be(ownerId);

            // ---- Close and archive the investigation itself (no regulatory filing
            // required for FirstAidOnly severity) -----------------------------------
            afterVerify.CloseInvestigation(ownerId, UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            await repository.UpdateAsync(afterVerify, CancellationToken.None);

            var afterClose = await repository.GetByIdAsync(accident.AccidentId, TenantId, CancellationToken.None);
            afterClose.Should().NotBeNull();
            afterClose!.Status.ToString().Should().Be("Closed");

            afterClose.Archive(UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            await repository.UpdateAsync(afterClose, CancellationToken.None);

            var afterArchive = await repository.GetByIdAsync(accident.AccidentId, TenantId, CancellationToken.None);
            afterArchive.Should().NotBeNull();
            afterArchive!.Status.ToString().Should().Be("Archived", "the real usp_Accident_Update must have persisted the Archived status");
            afterArchive.CorrectiveActions.Should().ContainSingle(a => a.Status.ToString() == "VerifiedEffective",
                "the CorrectiveAction child row must retain its verified status through the parent's remaining lifecycle transitions");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
