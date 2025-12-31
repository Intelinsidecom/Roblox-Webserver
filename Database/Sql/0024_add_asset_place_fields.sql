-- 0024_add_asset_place_fields.sql
-- Extend assets with a boolean is_place flag and a simple privacy_level enum
-- for place-type assets (1 = Public, 2 = Friends, 3 = Private).

alter table if exists assets
    add column if not exists is_place boolean not null default false;

alter table if exists assets
    add column if not exists privacy_level smallint not null default 1;

create index if not exists idx_assets_is_place on assets(is_place);
create index if not exists idx_assets_privacy_level on assets(privacy_level);
