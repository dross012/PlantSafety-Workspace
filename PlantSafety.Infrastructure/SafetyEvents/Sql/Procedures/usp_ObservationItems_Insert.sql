-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.Observation
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ObservationItems_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Observation aggregate root row. Backs
--                  IObservationRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging per
--                  the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  ObservedAtUtc serves as CreatedAtUtc and ObservedBy serves as both ObserverId
--                  and CreatedBy (always equal at creation). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_ObservationItems_Insert]
    @ObservationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @ObservationType NVARCHAR(50),
    @Category NVARCHAR(50),
    @Location NVARCHAR(400),
    @Description NVARCHAR(2000),
    @CorrectiveActionRequired BIT,
    @FollowUpDueAtUtc DATETIME2(3) = NULL,
    @ObservedAtUtc DATETIME2(3),
    @ObservedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[ObservationItems]
        (
            ObservationId, TenantId, SiteId, Status, ObservationType, Category, Location, Description,
            CorrectiveActionRequired, FollowUpDueAtUtc, ObservedAtUtc, ObservedBy, IsDeleted
        )
        VALUES
        (
            @ObservationId, @TenantId, @SiteId, @Status, @ObservationType, @Category, @Location, @Description,
            @CorrectiveActionRequired, @FollowUpDueAtUtc, @ObservedAtUtc, @ObservedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ObservationItems', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ObservationId=', @ObservationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; User=', SYSTEM_USER);
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
