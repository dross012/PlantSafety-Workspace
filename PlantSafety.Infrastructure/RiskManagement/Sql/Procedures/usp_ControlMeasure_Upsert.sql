-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ControlMeasure_Upsert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new ControlMeasure row, or updates it if already present (matched by
--                  ControlMeasureId). Used by IRiskRepository.UpdateAsync to reconcile the
--                  in-memory Risk.ControlMeasures collection -- control measures are immutable
--                  once applied (no lifecycle mutation methods on the entity), so this is
--                  effectively insert-only in practice but implemented as an upsert for safety.
--                  Additive-only evolution.
--  UPDATED:        2026-07-28 -- returns the post-upsert row's ETag (CONVERT(NVARCHAR(34),
--                  CAST(RowVersion AS BINARY(8)), 1)) via a trailing SELECT so callers have an
--                  up-to-date ETag immediately after insert/update without a separate round-trip.
--                  Schema-only: no @ETag input parameter or conflict-rejection logic is introduced.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_ControlMeasure_Upsert]
    @ControlMeasureId    UNIQUEIDENTIFIER,
    @RiskId              UNIQUEIDENTIFIER,
    @TenantId            UNIQUEIDENTIFIER,
    @HierarchyLevel      NVARCHAR(30),
    @Description         NVARCHAR(2000),
    @Justification       NVARCHAR(2000) = NULL,
    @ResponsiblePersonId UNIQUEIDENTIFIER,
    @ImplementedAtUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        DECLARE @__IsInsert BIT = CASE WHEN EXISTS (SELECT 1 FROM [RiskMgmt].[ControlMeasure] WHERE ControlMeasureId = @ControlMeasureId) THEN 0 ELSE 1 END;

        IF @__IsInsert = 0
        BEGIN
            UPDATE [RiskMgmt].[ControlMeasure]
            SET HierarchyLevel = @HierarchyLevel, Description = @Description, Justification = @Justification
            WHERE ControlMeasureId = @ControlMeasureId;
        END
        ELSE
        BEGIN
            INSERT INTO [RiskMgmt].[ControlMeasure]
                (ControlMeasureId, RiskId, TenantId, HierarchyLevel, Description, Justification, ResponsiblePersonId, ImplementedAtUtc, IsDeleted)
            VALUES
                (@ControlMeasureId, @RiskId, @TenantId, @HierarchyLevel, @Description, @Justification, @ResponsiblePersonId, @ImplementedAtUtc, 0);
        END

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ControlMeasure', DB_NAME(), CASE WHEN @__IsInsert = 1 THEN N'INSERT' ELSE N'UPDATE' END, SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ControlMeasureId=', @ControlMeasureId, N'; RiskId=', @RiskId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [RiskMgmt].[ControlMeasure]
        WHERE ControlMeasureId = @ControlMeasureId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ControlMeasureId=', @ControlMeasureId, N'; RiskId=', @RiskId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
