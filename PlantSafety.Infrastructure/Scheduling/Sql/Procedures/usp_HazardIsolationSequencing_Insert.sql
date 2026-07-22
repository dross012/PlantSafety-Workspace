-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_HazardIsolationSequencing_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new HazardIsolationSequencing aggregate root row. Backs IHazardIsolationSequencingRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical
--                  write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_HazardIsolationSequencing_Insert]
    @HazardIsolationSequencingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @HazardZone NVARCHAR(400),
    @TaskType NVARCHAR(400),
    @WindowStartUtc DATETIME2(3) = NULL,
    @WindowEndUtc DATETIME2(3) = NULL,
    @ConflictDetected BIT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Scheduling].[HazardIsolationSequencing]
        (
            HazardIsolationSequencingId, TenantId, SiteId, Status, HazardZone, TaskType, WindowStartUtc, WindowEndUtc, ConflictDetected, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @HazardIsolationSequencingId, @TenantId, @SiteId, @Status, @HazardZone, @TaskType, @WindowStartUtc, @WindowEndUtc, @ConflictDetected, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'HazardIsolationSequencing', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'HazardIsolationSequencingId=', @HazardIsolationSequencingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HazardIsolationSequencingId=', @HazardIsolationSequencingId, N'; User=', SYSTEM_USER);
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
