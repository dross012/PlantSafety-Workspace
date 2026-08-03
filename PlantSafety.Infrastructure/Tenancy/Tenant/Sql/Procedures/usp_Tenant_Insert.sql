-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Tenancy.Tenant
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Tenant_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
--  SchemaVersion:  2026.1
--  Description:    Inserts a newly provisioned Tenant row. Backs ITenantRepository.AddAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Tenancy].[usp_Tenant_Insert]
    @TenantId          UNIQUEIDENTIFIER,
    @Name              NVARCHAR(200),
    @Region            NVARCHAR(50),
    @Status            NVARCHAR(20),
    @PlanCode          NVARCHAR(30),
    @ConfigurationJson NVARCHAR(MAX),
    @CreatedAtUtc      DATETIME2(3),
    @ActivatedAtUtc    DATETIME2(3) = NULL,
    @SuspendedAtUtc    DATETIME2(3) = NULL,
    @SuspensionReason  NVARCHAR(2000) = NULL,
    @ArchivedAtUtc     DATETIME2(3) = NULL,
    @DeletedAtUtc      DATETIME2(3) = NULL,
    @DeletionReason    NVARCHAR(2000) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Tenancy].[Tenant]
        (TenantId, Name, Region, Status, PlanCode, ConfigurationJson, CreatedAtUtc,
         ActivatedAtUtc, SuspendedAtUtc, SuspensionReason, ArchivedAtUtc, DeletedAtUtc, DeletionReason)
        VALUES
        (@TenantId, @Name, @Region, @Status, @PlanCode, @ConfigurationJson, @CreatedAtUtc,
         @ActivatedAtUtc, @SuspendedAtUtc, @SuspensionReason, @ArchivedAtUtc, @DeletedAtUtc, @DeletionReason);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Tenant', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'TenantId=', @TenantId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
