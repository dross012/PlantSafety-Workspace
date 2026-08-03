-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Tenancy.Tenant
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_TenantAdminRoleAssignment_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
--  SchemaVersion:  2026.1
--  Description:    Records the Tenancy-scoped audit trail of a role grant made during tenant
--                  provisioning. Backs ITenantRoleAssigner. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Tenancy].[usp_TenantAdminRoleAssignment_Insert]
    @TenantId           UNIQUEIDENTIFIER,
    @UserId              UNIQUEIDENTIFIER,
    @GrantedRole         NVARCHAR(50),
    @InheritedRoleNames  NVARCHAR(500),
    @CorrelationId       UNIQUEIDENTIFIER,
    @GrantedAtUtc        DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Tenancy].[TenantAdminRoleAssignment]
        (TenantId, UserId, GrantedRole, InheritedRoleNames, CorrelationId, GrantedAtUtc)
        VALUES
        (@TenantId, @UserId, @GrantedRole, @InheritedRoleNames, @CorrelationId, @GrantedAtUtc);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'TenantAdminRoleAssignment', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'TenantId=', @TenantId, N'; UserId=', @UserId), 0, OBJECT_NAME(@@PROCID));
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
