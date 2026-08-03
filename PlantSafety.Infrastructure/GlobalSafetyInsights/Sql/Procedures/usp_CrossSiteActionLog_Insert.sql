-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         GlobalSafetyInsights
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CrossSiteActionLog_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
--  SchemaVersion:  2026.1
--  Description:    Inserts a cross-site coordinated-action audit ledger entry. Backs
--                  IGlobalSafetyActionAuditRepository.RecordAsync. TRY/CATCH with AuditDB/ErrorDB
--                  logging per the canonical write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [GlobalSafetyInsights].[usp_CrossSiteActionLog_Insert]
    @CrossSiteActionLogId UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @AffectedSiteIdsJson  NVARCHAR(MAX),
    @ActionType           NVARCHAR(100),
    @Description          NVARCHAR(2000) = NULL,
    @InitiatedByUserId    UNIQUEIDENTIFIER = NULL,
    @RecordedAtUtc        DATETIME2(3),
    @RegionCode           NVARCHAR(20) = NULL,
    @WorkflowInstanceId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [GlobalSafetyInsights].[CrossSiteActionLog]
        (
            CrossSiteActionLogId, TenantId, AffectedSiteIdsJson, ActionType, Description,
            InitiatedByUserId, RecordedAtUtc, RegionCode, WorkflowInstanceId
        )
        VALUES
        (
            @CrossSiteActionLogId, @TenantId, @AffectedSiteIdsJson, @ActionType, @Description,
            @InitiatedByUserId, @RecordedAtUtc, @RegionCode, @WorkflowInstanceId
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CrossSiteActionLog', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CrossSiteActionLogId=', @CrossSiteActionLogId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CrossSiteActionLogId=', @CrossSiteActionLogId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
