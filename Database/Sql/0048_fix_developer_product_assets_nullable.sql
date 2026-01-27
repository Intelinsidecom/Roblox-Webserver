-- 0043_fix_developer_product_assets_nullable.sql
-- Fix developer_product_assets table to allow NULL for developer_product_id

-- Make developer_product_id nullable to allow uploads before product creation
ALTER TABLE developer_product_assets ALTER COLUMN developer_product_id DROP NOT NULL;
