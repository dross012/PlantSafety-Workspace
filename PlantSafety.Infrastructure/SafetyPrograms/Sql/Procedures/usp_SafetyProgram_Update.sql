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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
