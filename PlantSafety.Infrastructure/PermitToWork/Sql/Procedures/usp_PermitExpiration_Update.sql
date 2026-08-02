-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitExpiration_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PermitExpiration aggregate root row.
--                  Backs IPermitExpirationRepository.UpdateAsync. Additive-only evolution.
--  UPDATED:        2026-07-29 -- Added @ETag optimistic-concurrency guard against the native
--                  RowVersion column (Gap 2 remediation; mirrors usp_Incident_Update). @@ROWCOUNT
--                  is captured into @RowsAffected immediately after the UPDATE and returned via
--                  an explicit SELECT -- callers MUST invoke this proc via ExecuteScalarAsync
--                  (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery
--                  always return -1, silently disabling concurrency enforcement.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitExpiration_Update]
    @PermitExpirationId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @PermitNumber NVARCHAR(400),
    @IssuedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @ExtensionRequested BIT,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [PermitToWork].[PermitExpiration] WHERE PermitExpirationId = @PermitExpirationId AND IsDeleted = 0)
            THROW 50004, N'PermitExpiration not found or has been deleted.', 1;

        UPDATE [PermitToWork].[PermitExpiration]
        SET
            Status = @Status,
            PermitNumber = @PermitNumber,
            IssuedAtUtc = @IssuedAtUtc,
            ExpiresAtUtc = @ExpiresAtUtc,
            ExtensionRequested = @ExtensionRequested,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE PermitExpirationId = @PermitExpirationId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PermitExpiration', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PermitExpirationId=', @PermitExpirationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitExpirationId=', @PermitExpirationId, N'; User=', SYSTEM_USER);
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
