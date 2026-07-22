-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistCompletion_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [Checklists].[ChecklistCompletion], backing
--                  IChecklistCompletionRepository.UpdateAsync (Start/RecordItemResult/
--                  FlagFailedItem/Complete lifecycle transitions). Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistCompletion_Update]
    @TenantId               UNIQUEIDENTIFIER,
    @CorrelationId          UNIQUEIDENTIFIER,
    @ActorId                UNIQUEIDENTIFIER,
    @NowUtc                 DATETIME2(3),
    @ChecklistCompletionId  UNIQUEIDENTIFIER,
    @Status                 NVARCHAR(20),
    @ItemsTotal             INT,
    @ItemsPassed            INT,
    @ItemsFailed            INT,
    @Outcome                NVARCHAR(30),
    @StartedAtUtc           DATETIME2(3) = NULL,
    @CompletedAtUtc         DATETIME2(3) = NULL,
    @CriticalFailureCount   INT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [Checklists].[ChecklistCompletion]
        SET [Status]                = @Status,
            [ItemsTotal]             = @ItemsTotal,
            [ItemsPassed]            = @ItemsPassed,
            [ItemsFailed]            = @ItemsFailed,
            [Outcome]                = @Outcome,
            [StartedAtUtc]           = @StartedAtUtc,
            [CompletedAtUtc]         = @CompletedAtUtc,
            [CriticalFailureCount]   = @CriticalFailureCount,
            [LastModifiedAtUtc]      = @NowUtc
        WHERE [ChecklistCompletionId] = @ChecklistCompletionId
          AND [TenantId]              = @TenantId
          AND ISNULL([IsDeleted], 0)  = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
