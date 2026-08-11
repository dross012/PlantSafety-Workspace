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
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ChecklistCompletion', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ChecklistCompletionId=', @ChecklistCompletionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ChecklistCompletionId=', @ChecklistCompletionId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
