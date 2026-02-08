-- 0050_add_universe_privacy_level.sql
-- Add privacy_level field to universes table for controlling universe visibility
-- Privacy levels: 1 = Public, 2 = Friends, 3 = Private

alter table if exists universes
    add column if not exists privacy_level smallint not null default 3;

alter table if exists universes
    add column if not exists Studio_Access_To_APIs boolean not null default false;

alter table if exists universes
    drop constraint if exists chk_universes_privacy_level;
alter table if exists universes
    add constraint chk_universes_privacy_level 
    check (privacy_level in (1, 2, 3));

create index if not exists idx_universes_privacy_level 
    on universes(privacy_level);

create index if not exists idx_universes_studio_api_access 
    on universes(Studio_Access_To_APIs) where Studio_Access_To_APIs = true;
