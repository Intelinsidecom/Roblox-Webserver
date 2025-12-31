-- 0025_add_user_owned_place_universe.sql
-- Adds owned_places and owned_universes jsonb arrays to users for faster lookup
-- of a user's places and universes.

alter table if exists users
    add column if not exists owned_places jsonb not null default '[]'::jsonb;

alter table if exists users
    add column if not exists owned_universes jsonb not null default '[]'::jsonb;

create index if not exists idx_users_owned_places on users using gin (owned_places);
create index if not exists idx_users_owned_universes on users using gin (owned_universes);
