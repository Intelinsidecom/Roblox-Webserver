-- 0056_create_user_downvoted_assets.sql
-- Create table to track which assets users have downvoted.

-- Drop a stale partial table before recreating so a prior failed run can't
-- trip up the FK/CHECK/trigger adds below.
DROP TABLE IF EXISTS user_downvoted_assets CASCADE;

CREATE TABLE IF NOT EXISTS user_downvoted_assets (
    id bigserial PRIMARY KEY,
    user_id bigint NOT NULL,
    asset_id bigint NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE(user_id, asset_id)
);

-- Drop-then-add for every FK and CHECK keeps the migration idempotent.
ALTER TABLE user_downvoted_assets
    DROP CONSTRAINT IF EXISTS fk_user_downvoted_assets_user;
ALTER TABLE user_downvoted_assets
    ADD CONSTRAINT fk_user_downvoted_assets_user
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE;

ALTER TABLE user_downvoted_assets
    DROP CONSTRAINT IF EXISTS fk_user_downvoted_assets_asset;
ALTER TABLE user_downvoted_assets
    ADD CONSTRAINT fk_user_downvoted_assets_asset
    FOREIGN KEY (asset_id) REFERENCES assets(asset_id) ON DELETE CASCADE;

CREATE INDEX IF NOT EXISTS idx_user_downvoted_assets_user ON user_downvoted_assets(user_id);
CREATE INDEX IF NOT EXISTS idx_user_downvoted_assets_asset ON user_downvoted_assets(asset_id);
CREATE INDEX IF NOT EXISTS idx_user_downvoted_assets_created ON user_downvoted_assets(created_at);

ALTER TABLE user_downvoted_assets
    DROP CONSTRAINT IF EXISTS chk_user_downvoted_assets_valid_ids;
ALTER TABLE user_downvoted_assets
    ADD CONSTRAINT chk_user_downvoted_assets_valid_ids
    CHECK (user_id > 0 AND asset_id > 0);

-- Function: CREATE OR REPLACE is already idempotent.
CREATE OR REPLACE FUNCTION update_asset_vote_counts()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_TABLE_NAME = 'user_upvoted_assets' THEN
        IF TG_OP = 'INSERT' THEN
            UPDATE assets SET upvotes = upvotes + 1 WHERE asset_id = NEW.asset_id;
        ELSIF TG_OP = 'DELETE' THEN
            UPDATE assets SET upvotes = upvotes - 1 WHERE asset_id = OLD.asset_id;
        END IF;
    ELSIF TG_TABLE_NAME = 'user_downvoted_assets' THEN
        IF TG_OP = 'INSERT' THEN
            UPDATE assets SET downvotes = downvotes + 1 WHERE asset_id = NEW.asset_id;
        ELSIF TG_OP = 'DELETE' THEN
            UPDATE assets SET downvotes = downvotes - 1 WHERE asset_id = OLD.asset_id;
        END IF;
    END IF;

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- DROP TRIGGER IF EXISTS makes trigger creation idempotent across reruns.
DROP TRIGGER IF EXISTS trigger_update_asset_upvotes ON user_upvoted_assets;
CREATE TRIGGER trigger_update_asset_upvotes
    AFTER INSERT OR DELETE ON user_upvoted_assets
    FOR EACH ROW EXECUTE FUNCTION update_asset_vote_counts();

DROP TRIGGER IF EXISTS trigger_update_asset_downvotes ON user_downvoted_assets;
CREATE TRIGGER trigger_update_asset_downvotes
    AFTER INSERT OR DELETE ON user_downvoted_assets
    FOR EACH ROW EXECUTE FUNCTION update_asset_vote_counts();
