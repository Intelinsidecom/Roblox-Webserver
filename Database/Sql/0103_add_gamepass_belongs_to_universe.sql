-- 0103_add_gamepass_belongs_to_universe.sql
-- Links game pass assets (asset_type_id = 34) to the universe/place they were
-- created for. targetPlaceId in the build flow is the universe's root_place_id,
-- so resolution is: universe_id via universes.root_place_id.

alter table if exists assets
    add column if not exists belongs_to_universe bigint null references universes(universe_id) on delete set null;

create index if not exists idx_assets_belongs_to_universe on assets(belongs_to_universe);
create index if not exists idx_assets_owner_type on assets(owner_user_id, asset_type_id);
