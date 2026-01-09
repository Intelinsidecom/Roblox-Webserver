-- 0028_add_asset_generated_icon_fields.sql
-- Add generated icon fields to assets table for place icons

alter table if exists assets
    add column if not exists generated_icon boolean not null default true;

alter table if exists assets
    add column if not exists place_generated_icon_url text null;

alter table if exists assets
    add column if not exists place_generated_icon_hash text null;

create index if not exists idx_assets_generated_icon on assets(generated_icon);
create index if not exists idx_assets_place_generated_icon_url on assets(place_generated_icon_url) where place_generated_icon_url is not null;
create index if not exists idx_assets_place_generated_icon_hash on assets(place_generated_icon_hash) where place_generated_icon_hash is not null;
