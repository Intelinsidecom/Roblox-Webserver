-- 0026_add_universe_thumbnail_url.sql
-- Add thumbnail_url field to universes table for storing game/place thumbnails

alter table if exists universes
    add column if not exists thumbnail_url text;

create index if not exists idx_universes_thumbnail_url on universes(thumbnail_url) where thumbnail_url is not null;
