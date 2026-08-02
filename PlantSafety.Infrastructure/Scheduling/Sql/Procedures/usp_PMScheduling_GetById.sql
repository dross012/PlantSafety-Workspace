-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PMScheduling_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PMScheduling row by identity. Backs IPMSchedulingRepository.GetByIdAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-30 (Phase 4 Scheduling.PMScheduling remediation) -- added @TenantId
--                  (cross-tenant access remediation): this proc previously filtered only by
--                  PMSchedulingId/IsDeleted, so any caller who knew or guessed another tenant's
--                  PMSchedulingId could read that tenant's record. Now requires and enforces
--                  WHERE TenantId = @TenantId; a cross-tenant lookup returns no row, which callers
--                  surface as NotFound rather than a security exception. Also added ETag, projected
--                  from the RowVersion column: CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_PMScheduling_GetById]
    @PMSchedulingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            PMSchedulingId, TenantId, SiteId, Status, EquipmentId, EquipmentDescription, PmTaskType,
            DueAtUtc, IsOverdue, EscalationPriorityCode, CompletionNotes, CreatedAtUtc, CreatedBy,
            UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Scheduling].[PMScheduling]
        WHERE PMSchedulingId = @PMSchedulingId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PMSchedulingId=', @PMSchedulingId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
