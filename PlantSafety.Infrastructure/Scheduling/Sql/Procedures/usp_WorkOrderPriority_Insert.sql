-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrderPriority_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new WorkOrderPriority aggregate root row. Backs IWorkOrderPriorityRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical
--                  write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_WorkOrderPriority_Insert]
    @WorkOrderPriorityId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @PriorityCode NVARCHAR(400),
    @HazardDescription NVARCHAR(400),
    @DueByUtc DATETIME2(3) = NULL,
    @EscalatedAtUtc DATETIME2(3) = NULL,
    @ResolutionNotes NVARCHAR(400) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Scheduling].[WorkOrderPriority]
        (
            WorkOrderPriorityId, TenantId, SiteId, Status, PriorityCode, HazardDescription, DueByUtc, EscalatedAtUtc, ResolutionNotes, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @WorkOrderPriorityId, @TenantId, @SiteId, @Status, @PriorityCode, @HazardDescription, @DueByUtc, @EscalatedAtUtc, @ResolutionNotes, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkOrderPriority', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkOrderPriorityId=', @WorkOrderPriorityId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkOrderPriorityId=', @WorkOrderPriorityId, N'; User=', SYSTEM_USER);
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
