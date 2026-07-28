-- 0094_fix_trade_constraints.sql
-- Fixes trade system constraints to support multiple copies of same asset (limited serials).
-- 1. Removes overly restrictive UNIQUE constraints that prevent owning multiple copies of same asset
-- 2. Adds serial_number to trade_items so transfers target the correct serial

-- Remove UNIQUE constraint from user_assets that prevents multiple copies of same asset
ALTER TABLE user_assets DROP CONSTRAINT IF EXISTS user_assets_user_id_asset_id_key;

-- Remove unique index from asset_serials that prevents owning multiple serials of same asset
DROP INDEX IF EXISTS idx_asset_serials_asset_owner;

-- Add serial_number to trade_items so we can identify which specific serial to transfer
ALTER TABLE trade_items ADD COLUMN IF NOT EXISTS serial_number BIGINT;

-- Backfill serial_number from asset_serials for existing trade items
UPDATE trade_items ti
SET serial_number = aser.serial_number
FROM asset_serials aser
WHERE ti.serial_number IS NULL
  AND aser.asset_id = ti.asset_id
  AND aser.owner_user_id = ti.agent_id;
