-- 0051_add_asset_universe_field.sql
-- Add in_universe boolean field to track if an asset is currently in a universe

alter table if exists assets
    add column if not exists in_universe boolean not null default false;

-- Create index for performance
create index if not exists idx_assets_in_universe on assets(in_universe);

