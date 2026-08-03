-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditReport
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AuditReport_GetForTarget.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves the AuditReport generation history for a target entity, most
--                  recent first. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditReport_GetForTarget]
    @TenantId         UNIQUEIDENTIFIER,
    @TargetEntityType NVARCHAR(200),
    @TargetEntityId   UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT AuditReportId, TenantId, SiteId, TargetEntityType, TargetEntityId, Format, Status,
               ContentSizeBytes, RenderedContentReference, FailureReason, RequestedBy, RequestedAtUtc, CompletedAtUtc
        FROM [Auditing].[AuditReport]
        WHERE TenantId = @TenantId AND TargetEntityType = @TargetEntityType AND TargetEntityId = @TargetEntityId
        ORDER BY RequestedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TargetEntityId=', @TargetEntityId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
