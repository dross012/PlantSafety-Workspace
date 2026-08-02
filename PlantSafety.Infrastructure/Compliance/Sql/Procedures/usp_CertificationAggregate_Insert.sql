-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CertificationAggregate_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Certification aggregate root row. Backs
--                  ICertificationRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging per
--                  the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationAggregate_Insert]
    @CertificationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @HolderWorkerId UNIQUEIDENTIFIER,
    @CertificationName NVARCHAR(400),
    @CertificationBody NVARCHAR(400),
    @CertificationNumber NVARCHAR(200),
    @IssuedAtUtc DATETIME2(3),
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @RenewalRequired BIT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[CertificationAggregate]
        (
            CertificationId, TenantId, SiteId, Status, HolderWorkerId, CertificationName, CertificationBody,
            CertificationNumber, IssuedAtUtc, ExpiresAtUtc, RenewalRequired, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @CertificationId, @TenantId, @SiteId, @Status, @HolderWorkerId, @CertificationName, @CertificationBody,
            @CertificationNumber, @IssuedAtUtc, @ExpiresAtUtc, @RenewalRequired, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CertificationAggregate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CertificationId=', @CertificationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CertificationId=', @CertificationId, N'; User=', SYSTEM_USER);
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
