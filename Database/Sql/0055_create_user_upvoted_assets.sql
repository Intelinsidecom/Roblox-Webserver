-- 0055_create_user_upvoted_assets.sql
-- Create table to track which assets users have upvoted.

-- Drop a stale partial table before recreating so any leftover from a prior
-- failed run (missing columns, missing constraints, etc.) can't trip up the
-- FK/CHECK adds below.
DROP TABLE IF EXISTS user_upvoted_assets CASCADE;

CREATE TABLE IF NOT EXISTS user_upvoted_assets (
    id bigserial PRIMARY KEY,
    user_id bigint NOT NULL,
    asset_id bigint NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE(user_id, asset_id)
);

-- Drop-then-add keeps this idempotent for both reruns and a partially-applied
-- schema where the constraints may or may not already exist.
ALTER TABLE user_upvoted_assets
    DROP CONSTRAINT IF EXISTS fk_user_upvoted_assets_user;
ALTER TABLE user_upvoted_assets
    ADD CONSTRAINT fk_user_upvoted_assets_user
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE;

ALTER TABLE user_upvoted_assets
    DROP CONSTRAINT IF EXISTS fk_user_upvoted_assets_asset;
ALTER TABLE user_upvoted_assets
    ADD CONSTRAINT fk_user_upvoted_assets_asset
    FOREIGN KEY (asset_id) REFERENCES assets(asset_id) ON DELETE CASCADE;

CREATE INDEX IF NOT EXISTS idx_user_upvoted_assets_user ON user_upvoted_assets(user_id);
CREATE INDEX IF NOT EXISTS idx_user_upvoted_assets_asset ON user_upvoted_assets(asset_id);
CREATE INDEX IF NOT EXISTS idx_user_upvoted_assets_created ON user_upvoted_assets(created_at);

ALTER TABLE user_upvoted_assets
    DROP CONSTRAINT IF EXISTS chk_user_upvoted_assets_valid_ids;
ALTER TABLE user_upvoted_assets
    ADD CONSTRAINT chk_user_upvoted_assets_valid_ids
    CHECK (user_id > 0 AND asset_id > 0);
