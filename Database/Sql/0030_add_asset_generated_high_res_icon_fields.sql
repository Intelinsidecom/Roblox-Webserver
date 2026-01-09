-- 0030_add_asset_generated_high_res_icon_fields.sql
-- Add high-resolution generated icon fields to assets table

alter table if exists assets
    add column if not exists place_generated_icon_high_res_url text null;

create index if not exists idx_assets_place_generated_high_res_url on assets(place_generated_icon_high_res_url) where place_generated_icon_high_res_url is not null;
