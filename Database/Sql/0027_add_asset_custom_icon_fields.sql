-- 0027_add_asset_custom_icon_fields.sql
-- Add custom icon fields to assets table for place icons

alter table if exists assets
    add column if not exists custom_icon boolean not null default false;

alter table if exists assets
    add column if not exists place_custom_icon_url text null;

alter table if exists assets
    add column if not exists place_custom_icon_hash text null;

create index if not exists idx_assets_custom_icon on assets(custom_icon);
create index if not exists idx_assets_place_custom_icon_url on assets(place_custom_icon_url) where place_custom_icon_url is not null;
create index if not exists idx_assets_place_custom_icon_hash on assets(place_custom_icon_hash) where place_custom_icon_hash is not null;
