-- 0105_add_xbox_xuid.sql
-- Adds Xbox Live XUID column to users table for account linking

ALTER TABLE users ADD COLUMN IF NOT EXISTS xbox_xuid TEXT;

CREATE UNIQUE INDEX IF NOT EXISTS idx_users_xbox_xuid ON users (xbox_xuid) WHERE xbox_xuid IS NOT NULL;