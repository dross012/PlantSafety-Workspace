-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.Users
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_User_GetById.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [EnterpriseFabric].[User] by identity. Backs
--                  IUserRepository.GetByIdAsync / UserDapperRepository.GetByIdAsync. Column list
--                  and aliases (RolesCsv AS RolesCsv, SiteIdsCsv AS SiteIdsCsv) match
--                  UserDapperRepository's private UserRow record exactly. ETag is projected from
--                  the native ROWVERSION (CONVERT(NVARCHAR(34), RowVersion, 1)), matching the
--                  concurrency-token convention established by usp_CorrectiveAction_GetById /
--                  usp_Process_GetById. TRY/CATCH + THROW only (no AuditLog/ErrorLog writes),
--                  matching the established read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_User_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @UserId   UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            UserId,
            TenantId,
            Email,
            FirstName,
            LastName,
            RolesCsv,
            SiteIdsCsv,
            IsActive,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [EnterpriseFabric].[User]
        WHERE UserId = @UserId
          AND TenantId = @TenantId;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
