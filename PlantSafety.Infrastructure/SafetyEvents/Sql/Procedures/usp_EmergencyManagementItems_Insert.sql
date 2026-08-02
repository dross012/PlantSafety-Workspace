-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.EmergencyManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EmergencyManagementItems_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new EmergencyManagement aggregate root row. Backs
--                  IEmergencyManagementRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB
--                  logging per the canonical write-procedure template (mirrors
--                  usp_Permits_Insert.sql). ReportedAtUtc/ReportedBy serve as both
--                  DeclaredAtUtc/CreatedAtUtc and CreatedBy respectively (always equal at
--                  creation). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_EmergencyManagementItems_Insert]
    @EmergencyId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @EmergencyType NVARCHAR(50),
    @Severity NVARCHAR(50),
    @Location NVARCHAR(400),
    @CasualtyCount INT,
    @ResponseTeamNotified BIT,
    @ReportedAtUtc DATETIME2(3),
    @StabilizedAtUtc DATETIME2(3) = NULL,
    @ResolvedAtUtc DATETIME2(3) = NULL,
    @ReportedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[EmergencyManagementItems]
        (
            EmergencyId, TenantId, SiteId, Status, EmergencyType, Severity, Location, CasualtyCount,
            ResponseTeamNotified, ReportedAtUtc, StabilizedAtUtc, ResolvedAtUtc, ReportedBy, IsDeleted
        )
        VALUES
        (
            @EmergencyId, @TenantId, @SiteId, @Status, @EmergencyType, @Severity, @Location, @CasualtyCount,
            @ResponseTeamNotified, @ReportedAtUtc, @StabilizedAtUtc, @ResolvedAtUtc, @ReportedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EmergencyManagementItems', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EmergencyId=', @EmergencyId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyId=', @EmergencyId, N'; User=', SYSTEM_USER);
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
