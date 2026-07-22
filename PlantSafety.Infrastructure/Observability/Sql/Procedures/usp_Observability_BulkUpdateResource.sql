-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Observability_BulkUpdateResource.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Applies a single Status transition to one resource, searching
--                  across every Observability aggregate table. Backs IObservabilityBulkRepository.
--                  BulkUpdateAsync (one call per resource id). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_Observability_BulkUpdateResource]
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

        IF EXISTS (SELECT 1 FROM [Observability].[AutomatedIncidentDetection] WHERE AutomatedIncidentDetectionId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Observability].[AutomatedIncidentDetection]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE AutomatedIncidentDetectionId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Observability].[EnvironmentalAssetMonitoring] WHERE EnvironmentalAssetMonitoringId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Observability].[EnvironmentalAssetMonitoring]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE EnvironmentalAssetMonitoringId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Observability].[FacilityTrafficMonitoring] WHERE FacilityTrafficMonitoringId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Observability].[FacilityTrafficMonitoring]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE FacilityTrafficMonitoringId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Observability].[MachineProcessMonitoring] WHERE MachineProcessMonitoringId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Observability].[MachineProcessMonitoring]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE MachineProcessMonitoringId = @ResourceId;

            SET @Found = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM [Observability].[PPEBehavioralMonitoring] WHERE PPEBehavioralMonitoringId = @ResourceId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            UPDATE [Observability].[PPEBehavioralMonitoring]
            SET Status = @Operation, UpdatedBy = @ActorId, UpdatedAtUtc = @NowUtc, ConcurrencyToken = NEWID()
            WHERE PPEBehavioralMonitoringId = @ResourceId;

            SET @Found = 1;
        END

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Observability.BulkResource', DB_NAME(), @Operation, SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ResourceId=', @ResourceId), 0, OBJECT_NAME(@@PROCID));
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
