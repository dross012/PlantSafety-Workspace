-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Tenancy.Tenant
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Tenant_ListAll.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
--  SchemaVersion:  2026.1
--  Description:    Lists every registered Tenant, most recently created first.
--                  Platform-admin-only, cross-tenant operation -- gated at the Application/API
--                  layer (TenantV1Controller), not by SQL Row-Level-Security (see Tenant.sql's
--                  header for why [Tenancy].[Tenant] is excluded from that policy). Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Tenancy].[usp_Tenant_ListAll]
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT TenantId, Name, Region, Status, PlanCode, ConfigurationJson, CreatedAtUtc,
               ActivatedAtUtc, SuspendedAtUtc, SuspensionReason, ArchivedAtUtc, DeletedAtUtc, DeletionReason
        FROM [Tenancy].[Tenant]
        ORDER BY CreatedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'User=', SYSTEM_USER);
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
