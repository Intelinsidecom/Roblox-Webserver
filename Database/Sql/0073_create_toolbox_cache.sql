-- 0073_create_toolbox_cache.sql
-- Created: 2026-07-01
-- Pre-computed sorted asset ID lists for the Studio Toolbox.
-- Stores only asset_ids keyed by sort_type and asset_type_id.

CREATE TABLE IF NOT EXISTS toolbox_cache (
    id BIGSERIAL PRIMARY KEY,
    sort_type SMALLINT NOT NULL,
    asset_type_id SMALLINT NOT NULL,
    asset_id BIGINT NOT NULL REFERENCES assets(asset_id) ON DELETE CASCADE,
    sort_order INT NOT NULL,
    cached_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (sort_type, asset_type_id, sort_order)
);

CREATE INDEX IF NOT EXISTS idx_toolbox_cache_lookup ON toolbox_cache (sort_type, asset_type_id, sort_order);
CREATE INDEX IF NOT EXISTS idx_toolbox_cache_asset ON toolbox_cache (asset_id);
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX IF NOT EXISTS idx_assets_name_trgm ON assets USING gin (name gin_trgm_ops);
