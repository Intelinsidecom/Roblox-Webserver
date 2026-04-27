-- 0068_add_place_player_count.sql
-- Adds player_count column to assets table for tracking current players in a place

alter table assets 
    add column if not exists player_count integer not null default 0;

create index if not exists idx_assets_player_count on assets (player_count desc);
