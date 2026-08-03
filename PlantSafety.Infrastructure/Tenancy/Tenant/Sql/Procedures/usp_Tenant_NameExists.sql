-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Tenancy.Tenant
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Tenant_NameExists.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
--  SchemaVersion:  2026.1
--  Description:    Returns 1 if a non-deleted Tenant with the given name already exists, else 0.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Tenancy].[usp_Tenant_NameExists]
    @Name NVARCHAR(200)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT CASE WHEN EXISTS (
            SELECT 1 FROM [Tenancy].[Tenant] WHERE Name = @Name AND DeletedAtUtc IS NULL
        ) THEN 1 ELSE 0 END;
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
