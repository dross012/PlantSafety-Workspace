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
--  UPDATED:        2026-07-30 -- Phase 4 full build-out: @TenantId is now part of the WHERE clause
--                  (tenant-scoped write) and @ETag enforces optimistic concurrency against the
--                  native RowVersion column, mirroring usp_Permits_Update.sql. @RowsAffected is
--                  captured via @@ROWCOUNT immediately after the UPDATE (before the audit INSERT,
--                  which would otherwise reset it) and returned via SELECT -- callers MUST invoke
--                  this proc via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since
--                  SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently
--                  disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_EmergencyDrillPlanning_Update]
    @EmergencyDrillPlanningId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @DrillType NVARCHAR(400),
    @ShiftCode NVARCHAR(400),
    @ScheduledAtUtc DATETIME2(3) = NULL,
    @ParticipationCount INT,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Scheduling].[EmergencyDrillPlanning] WHERE EmergencyDrillPlanningId = @EmergencyDrillPlanningId AND TenantId = @TenantId AND IsDeleted = 0)
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
        WHERE EmergencyDrillPlanningId = @EmergencyDrillPlanningId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EmergencyDrillPlanning', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EmergencyDrillPlanningId=', @EmergencyDrillPlanningId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
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
