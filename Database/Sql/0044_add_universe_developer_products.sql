-- 0044_add_universe_developer_products.sql
-- Add developer_products JSON column to universes table for storing universe-level developer products
-- Description: This allows storing developer products at the universe level rather than place level
-- Created: 2025-01-25

ALTER TABLE IF EXISTS universes
    ADD COLUMN IF NOT EXISTS developer_products jsonb NOT NULL DEFAULT '[]'::jsonb;

-- Create index for efficient querying of developer products within universes
CREATE INDEX IF NOT EXISTS idx_universes_developer_products 
    ON universes USING gin (developer_products);
