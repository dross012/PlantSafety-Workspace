-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EsgDisclosureReport_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new EsgDisclosureReport row (Draft, version N). Backs
--                  IEsgDisclosureReportRepository.AddAsync (used for both the version-1
--                  Generate() path and each CreateSupersedingVersion() follow-on row).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_EsgDisclosureReport_Insert]
    @EsgDisclosureReportId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @SourceLedgerId UNIQUEIDENTIFIER,
    @Frameworks NVARCHAR(100),
    @ReportingPeriodStartUtc DATETIME2(3),
    @ReportingPeriodEndUtc DATETIME2(3),
    @Version INT,
    @SupersedesReportId UNIQUEIDENTIFIER = NULL,
    @MethodologyVersion NVARCHAR(100),
    @Status NVARCHAR(20),
    @GeneratedAtUtc DATETIME2(3),
    @GeneratedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SustainabilityAdvancedMetrics].[EsgDisclosureReport]
        (EsgDisclosureReportId, TenantId, SiteId, SourceLedgerId, Frameworks, ReportingPeriodStartUtc, ReportingPeriodEndUtc, Version, SupersedesReportId, MethodologyVersion, Status, GeneratedAtUtc, GeneratedBy, IsDeleted)
        VALUES
        (@EsgDisclosureReportId, @TenantId, @SiteId, @SourceLedgerId, @Frameworks, @ReportingPeriodStartUtc, @ReportingPeriodEndUtc, @Version, @SupersedesReportId, @MethodologyVersion, @Status, @GeneratedAtUtc, @GeneratedBy, 0);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EsgDisclosureReport', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EsgDisclosureReportId=', @EsgDisclosureReportId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EsgDisclosureReportId=', @EsgDisclosureReportId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
