-- 0062_add_player_count_to_assets.sql
-- Adds player_count column to assets table for tracking current players in places

ALTER TABLE assets ADD COLUMN IF NOT EXISTS player_count integer NOT NULL DEFAULT 0;

CREATE INDEX IF NOT EXISTS idx_assets_player_count ON assets(player_count);
