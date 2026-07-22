-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EmergencyDrillPlanning_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a EmergencyDrillPlanning aggregate root row.
--                  Backs IEmergencyDrillPlanningRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_EmergencyDrillPlanning_Update]
    @EmergencyDrillPlanningId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @DrillType NVARCHAR(400),
    @ShiftCode NVARCHAR(400),
    @ScheduledAtUtc DATETIME2(3) = NULL,
    @ParticipationCount INT,
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Scheduling].[EmergencyDrillPlanning] WHERE EmergencyDrillPlanningId = @EmergencyDrillPlanningId AND IsDeleted = 0)
            THROW 50004, N'EmergencyDrillPlanning not found or has been deleted.', 1;

        UPDATE [Scheduling].[EmergencyDrillPlanning]
        SET
            Status = @Status,
            DrillType = @DrillType,
            ShiftCode = @ShiftCode,
            ScheduledAtUtc = @ScheduledAtUtc,
            ParticipationCount = @ParticipationCount,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE EmergencyDrillPlanningId = @EmergencyDrillPlanningId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EmergencyDrillPlanning', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EmergencyDrillPlanningId=', @EmergencyDrillPlanningId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyDrillPlanningId=', @EmergencyDrillPlanningId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
