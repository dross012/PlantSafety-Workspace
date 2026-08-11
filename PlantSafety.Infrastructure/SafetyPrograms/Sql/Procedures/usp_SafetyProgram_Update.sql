-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPrograms.SafetyProgram
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyProgram_Update.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Update procedure for [SafetyPrograms].[SafetyProgram], backing
--                  ISafetyProgramRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyPrograms].[usp_SafetyProgram_Update]
    @TenantId              UNIQUEIDENTIFIER,
    @CorrelationId         UNIQUEIDENTIFIER,
    @ActorId               UNIQUEIDENTIFIER,
    @NowUtc                DATETIME2(3),
    @SafetyProgramId       UNIQUEIDENTIFIER,
    @Status                NVARCHAR(20),
    @Name                  NVARCHAR(256),
    @Description           NVARCHAR(4000),
    @StartDate             DATETIME2(3),
    @EndDate               DATETIME2(3) = NULL,
    @ParticipationPercent  FLOAT,
    @EnrolledCount         INT,
    @Goals                 NVARCHAR(4000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyPrograms].[SafetyProgram]
        SET [Status] = @Status,
            [Name] = @Name,
            [Description] = @Description,
            [StartDate] = CAST(@StartDate AS DATE),
            [EndDate] = CAST(@EndDate AS DATE),
            [ParticipationPercent] = @ParticipationPercent,
            [EnrolledCount] = @EnrolledCount,
            [Goals] = @Goals,
            [LastModifiedAtUtc] = @NowUtc
        WHERE [SafetyProgramId] = @SafetyProgramId
          AND [TenantId] = @TenantId;
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'SafetyProgram', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'SafetyProgramId=', @SafetyProgramId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SafetyProgramId=', @SafetyProgramId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
