-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.Credentials
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Credentials_GetStatusItem.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing ICredentialReadRepository.GetStatusAsync
--                  (Application read-model). Badge number is masked at the source per the
--                  module's PII data-governance policy.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_GetStatusItem]
    @TenantId     UNIQUEIDENTIFIER,
    @CredentialId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CredentialId],
        [TenantId],
        ('****' + RIGHT([BadgeNumber], 4)) AS [BadgeNumberMasked],
        [HolderType],
        [AccessLevel],
        [Status],
        [IssuedAtUtc],
        [ExpiresAtUtc]
    FROM [PhysicalAccessControl].[Credentials]
    WHERE [TenantId] = @TenantId
      AND [CredentialId] = @CredentialId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
