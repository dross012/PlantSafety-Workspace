-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           OutboxMessages.sql
--  Created:        2026-06-17
--  Author:         David (Ted) + Copilot
--  SchemaVersion:  2026.1
--  Description:    Stores serialised domain events pending publish to Azure Service Bus.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'OutboxMessages')
BEGIN
    CREATE TABLE [System].[OutboxMessages] (
        -- TODO: Define columns per data dictionary
        Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
        CreatedAt   DATETIME2(3)     NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO
