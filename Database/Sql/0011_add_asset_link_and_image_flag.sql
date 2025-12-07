-- 0011_add_asset_link_and_image_flag.sql
-- Adds columns to indicate an asset is an image for another asset, and to store the linked asset id.

alter table if exists assets
    add column if not exists asset_image boolean not null default false,
    add column if not exists asset_link bigint null;

create index if not exists idx_assets_asset_link on assets(asset_link);
