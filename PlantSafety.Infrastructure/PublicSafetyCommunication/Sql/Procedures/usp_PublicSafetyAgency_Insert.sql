-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicSafetyAgency_Insert.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new PublicSafetyAgency aggregate root row. Backs
--                  IPublicSafetyAgencyRepository.AddAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_Insert]
    @PublicSafetyAgencyId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @AgencyName NVARCHAR(400),
    @AgencyType NVARCHAR(64),
    @JurisdictionDescription NVARCHAR(400),
    @PrimaryPhoneNumber NVARCHAR(64),
    @PrimaryEmail NVARCHAR(320) = NULL,
    @VerificationStatus NVARCHAR(50),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PublicSafetyCommunication].[PublicSafetyAgency]
        (
            PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
            PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @PublicSafetyAgencyId, @TenantId, @SiteId, @Status, @AgencyName, @AgencyType, @JurisdictionDescription,
            @PrimaryPhoneNumber, @PrimaryEmail, @VerificationStatus, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PublicSafetyAgency', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PublicSafetyAgencyId=', @PublicSafetyAgencyId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PublicSafetyAgencyId=', @PublicSafetyAgencyId, N'; User=', SYSTEM_USER);
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
