-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RegulatoryCalendar
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RegulatoryCalendar_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new RegulatoryCalendar aggregate root row. Backs
--                  IRegulatoryCalendarRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_RegulatoryCalendar_Insert]
    @RegulatoryCalendarId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @EventTitle NVARCHAR(400),
    @RegulatoryBody NVARCHAR(400),
    @JurisdictionCode NVARCHAR(50),
    @ComplianceCategory NVARCHAR(200),
    @ResponsiblePartyId UNIQUEIDENTIFIER,
    @DueAtUtc DATETIME2(3) = NULL,
    @ReminderLeadDays INT = 30,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[RegulatoryCalendar]
        (
            RegulatoryCalendarId, TenantId, SiteId, Status, EventTitle, RegulatoryBody, JurisdictionCode, ComplianceCategory, ResponsiblePartyId, DueAtUtc, ReminderLeadDays, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @RegulatoryCalendarId, @TenantId, @SiteId, @Status, @EventTitle, @RegulatoryBody, @JurisdictionCode, @ComplianceCategory, @ResponsiblePartyId, @DueAtUtc, @ReminderLeadDays, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'RegulatoryCalendar', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId, N'; User=', SYSTEM_USER);
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
