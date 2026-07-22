-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Scheduling_BulkUpdateResource.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Applies a single Status transition to one resource, searching
--                  across every Scheduling aggregate table. Backs ISchedulingBulkRepository.
--                  BulkUpdateAsync (one call per resource id). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_Scheduling_BulkUpdateResource]
    @ResourceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Operation NVARCHAR(50),
    @ActorId UNIQUEIDENTIFIER = NULL,
    @NowUtc DATETIME2(3),
    @Found BIT OUTPUT
AS
SET NOCOUNT ON;
BEGIN
    SET @Found = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM [Scheduling].[PMScheduling] WHERE PMSchedulingId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Scheduling].[PMScheduling]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE PMSchedulingId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Scheduling].[EmergencyDrillPlanning] WHERE EmergencyDrillPlanningId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Scheduling].[EmergencyDrillPlanning]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE EmergencyDrillPlanningId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Scheduling].[HazardIsolationSequencing] WHERE HazardIsolationSequencingId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Scheduling].[HazardIsolationSequencing]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE HazardIsolationSequencingId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Scheduling].[ShiftFatigue] WHERE ShiftFatigueId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Scheduling].[ShiftFatigue]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE ShiftFatigueId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Scheduling].[WorkOrderPriority] WHERE WorkOrderPriorityId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Scheduling].[WorkOrderPriority]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE WorkOrderPriorityId = @ResourceId;

            SET @Found = 1;
        END

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Scheduling.BulkResource', DB_NAME(), @Operation, SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ResourceId=', @ResourceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ResourceId=', @ResourceId, N'; Operation=', @Operation, N'; User=', SYSTEM_USER);
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
