-- 0049_add_in_game_permissions.sql
-- Add in-game permissions for places to control API access for copying and updating

-- Allow this place to be copied as a template using the Create Place API in your game
alter table if exists assets
    add column if not exists allow_place_to_be_copied_in_game boolean not null default false;

-- Allow this place to be updated using the Save Place API in your game  
alter table if exists assets
    add column if not exists allow_place_to_be_updated_in_game boolean not null default false;

-- Create indexes for performance on place assets
create index if not exists idx_assets_allow_copied_in_game on assets(allow_place_to_be_copied_in_game) where is_place = true;
create index if not exists idx_assets_allow_updated_in_game on assets(allow_place_to_be_updated_in_game) where is_place = true;
