-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveLoadLedger_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new CognitiveLoadLedger header row. Backs
--                  ICognitiveLoadLedgerRepository.AddAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_Insert]
    @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @ShiftType NVARCHAR(20),
    @ShiftDateUtc DATETIME2(3),
    @Status NVARCHAR(50),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
        (CognitiveLoadLedgerId, TenantId, SiteId, ShiftType, ShiftDateUtc, Status, CreatedAtUtc, CreatedBy, IsDeleted)
        VALUES
        (@CognitiveLoadLedgerId, @TenantId, @SiteId, @ShiftType, @ShiftDateUtc, @Status, @CreatedAtUtc, @CreatedBy, 0);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CognitiveLoadLedger', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CognitiveLoadLedgerId=', @CognitiveLoadLedgerId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CognitiveLoadLedgerId=', @CognitiveLoadLedgerId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
