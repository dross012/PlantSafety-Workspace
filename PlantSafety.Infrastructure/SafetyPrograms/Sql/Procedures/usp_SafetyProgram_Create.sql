-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPrograms.SafetyProgram
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyProgram_Create.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyPrograms].[SafetyProgram], backing
--                  ISafetyProgramRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyPrograms].[usp_SafetyProgram_Create]
    @TenantId              UNIQUEIDENTIFIER,
    @CorrelationId         UNIQUEIDENTIFIER,
    @ActorId               UNIQUEIDENTIFIER,
    @NowUtc                DATETIME2(3),
    @SafetyProgramId       UNIQUEIDENTIFIER,
    @SiteId                UNIQUEIDENTIFIER = NULL,
    @Status                NVARCHAR(20),
    @Name                  NVARCHAR(256),
    @Description           NVARCHAR(4000),
    @Category              NVARCHAR(40),
    @StartDate             DATETIME2(3),
    @EndDate               DATETIME2(3) = NULL,
    @OwnerId               UNIQUEIDENTIFIER,
    @ParticipationPercent  FLOAT,
    @EnrolledCount         INT,
    @Goals                 NVARCHAR(4000) = NULL,
    @CreatedAtUtc           DATETIME2(3),
    @CreatedBy              UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [SafetyPrograms].[SafetyProgram]
            ([SafetyProgramId], [TenantId], [SiteId], [Status], [Name], [Description], [Category],
             [StartDate], [EndDate], [OwnerId], [ParticipationPercent], [EnrolledCount], [Goals],
             [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@SafetyProgramId, @TenantId, @SiteId, @Status, @Name, @Description, @Category,
             CAST(@StartDate AS DATE), CAST(@EndDate AS DATE), @OwnerId, @ParticipationPercent, @EnrolledCount, @Goals,
             @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
