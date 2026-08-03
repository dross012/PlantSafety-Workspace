-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Tenancy.Tenant
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Tenant_GetByName.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single non-deleted Tenant row by name (case-insensitive, per
--                  default collation). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Tenancy].[usp_Tenant_GetByName]
    @Name NVARCHAR(200)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT TenantId, Name, Region, Status, PlanCode, ConfigurationJson, CreatedAtUtc,
               ActivatedAtUtc, SuspendedAtUtc, SuspensionReason, ArchivedAtUtc, DeletedAtUtc, DeletionReason
        FROM [Tenancy].[Tenant]
        WHERE Name = @Name AND DeletedAtUtc IS NULL;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'Name=', @Name, N'; User=', SYSTEM_USER);
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
