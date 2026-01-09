-- 0029_add_asset_high_res_icon_fields.sql
-- Add high-resolution custom icon fields to assets table

alter table if exists assets
    add column if not exists place_custom_icon_high_res_url text null;

create index if not exists idx_assets_place_custom_icon_high_res_url on assets(place_custom_icon_high_res_url) where place_custom_icon_high_res_url is not null;
