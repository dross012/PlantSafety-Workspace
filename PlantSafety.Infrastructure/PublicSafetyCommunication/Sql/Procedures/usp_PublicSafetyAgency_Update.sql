-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicSafetyAgency_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PublicSafetyAgency aggregate root row.
--                  Backs IPublicSafetyAgencyRepository.UpdateAsync. @ETag optimistic-concurrency
--                  guard against the native RowVersion column. Callers MUST invoke via
--                  ExecuteScalarAsync (see usp_PermitWorkflow_Update.sql remarks for rationale).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_Update]
    @PublicSafetyAgencyId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @AgencyName NVARCHAR(400),
    @AgencyType NVARCHAR(64),
    @JurisdictionDescription NVARCHAR(400),
    @PrimaryPhoneNumber NVARCHAR(64),
    @PrimaryEmail NVARCHAR(320) = NULL,
    @VerificationStatus NVARCHAR(50),
    @LastVerifiedAtUtc DATETIME2(3) = NULL,
    @NextVerificationDueAtUtc DATETIME2(3) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [PublicSafetyCommunication].[PublicSafetyAgency] WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId AND IsDeleted = 0)
            THROW 50004, N'PublicSafetyAgency not found or has been deleted.', 1;

        UPDATE [PublicSafetyCommunication].[PublicSafetyAgency]
        SET
            Status = @Status,
            AgencyName = @AgencyName,
            AgencyType = @AgencyType,
            JurisdictionDescription = @JurisdictionDescription,
            PrimaryPhoneNumber = @PrimaryPhoneNumber,
            PrimaryEmail = @PrimaryEmail,
            VerificationStatus = @VerificationStatus,
            LastVerifiedAtUtc = @LastVerifiedAtUtc,
            NextVerificationDueAtUtc = @NextVerificationDueAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PublicSafetyAgency', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PublicSafetyAgencyId=', @PublicSafetyAgencyId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
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
