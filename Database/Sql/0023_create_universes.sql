-- 0023_create_universes.sql
-- Basic universes table: one row per game universe. The place_ids column holds
-- the ids of all places that belong to this universe. The start/root place can
-- be chosen as the lowest id in that array or by convention the first element.

create table if not exists universes (
    universe_id     bigserial primary key,
    name            text        not null,
    creator_user_id bigint      not null,
    place_ids       bigint[]    not null default '{}'::bigint[],
    created_at      timestamptz not null default now()
);

create index if not exists idx_universes_creator on universes(creator_user_id);
