-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.StopWorkEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_StopWorkEvent_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyCulture].[StopWorkEvent], backing
--                  IStopWorkEventRepository.AddAsync. Standard error envelope: TRY/CATCH
--                  with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_StopWorkEvent_Create]
    @TenantId                     UNIQUEIDENTIFIER,
    @CorrelationId                UNIQUEIDENTIFIER,
    @ActorId                      UNIQUEIDENTIFIER,
    @NowUtc                       DATETIME2(3),
    @StopWorkEventId              UNIQUEIDENTIFIER,
    @SiteId                       UNIQUEIDENTIFIER = NULL,
    @Status                       NVARCHAR(20),
    @WorkArea                     NVARCHAR(200),
    @HazardDescription            NVARCHAR(2000),
    @InitiatedByActorKind         NVARCHAR(20),
    @InitiatedByUserId            UNIQUEIDENTIFIER = NULL,
    @WasCelebrated                BIT,
    @RelatedPermitId              UNIQUEIDENTIFIER = NULL,
    @RelatedChecklistCompletionId UNIQUEIDENTIFIER = NULL,
    @CreatedAtUtc                 DATETIME2(3),
    @CreatedBy                    UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [SafetyCulture].[StopWorkEvent]
            ([StopWorkEventId], [TenantId], [SiteId], [Status], [WorkArea], [HazardDescription],
             [InitiatedByActorKind], [InitiatedByUserId], [WasCelebrated],
             [RelatedPermitId], [RelatedChecklistCompletionId], [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@StopWorkEventId, @TenantId, @SiteId, @Status, @WorkArea, @HazardDescription,
             @InitiatedByActorKind, @InitiatedByUserId, @WasCelebrated,
             @RelatedPermitId, @RelatedChecklistCompletionId, @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
