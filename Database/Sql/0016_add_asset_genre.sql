-- 0016_add_asset_genre.sql
-- Adds a numeric genre field for assets (1-15).

alter table if exists assets
    add column if not exists genre integer not null default 1;
