-- 0010_add_asset_thumbnail_url.sql
-- Adds a column to store the full URL to an asset's thumbnail (e.g. T-Shirt thumbnails).

alter table if exists assets
    add column if not exists thumbnail_url text;
