-- 0104_create_private_servers.sql
-- Adds VIP (private) server support. One row per purchased VIP server; owners can
-- whitelist other users who may join. access_code is the secret token embedded in
-- join links / PlaceLauncher RequestPrivateGame calls.

create table if not exists private_servers (
    private_server_id bigserial primary key,
    universe_id       bigint      not null,
    place_id          bigint      not null,
    owner_user_id     bigint      not null,
    name              text        not null,
    access_code       text        not null unique,
    active            boolean     not null default true,
    auto_renew        boolean     not null default true,
    expires_at        timestamptz not null default now() + interval '30 days',
    created_at        timestamptz not null default now(),
    updated_at        timestamptz not null default now()
);

create index if not exists idx_private_servers_owner    on private_servers(owner_user_id);
create index if not exists idx_private_servers_place    on private_servers(place_id);
create index if not exists idx_private_servers_universe on private_servers(universe_id);

-- Users explicitly allowed to join a VIP server besides its owner.
create table if not exists private_server_whitelist (
    private_server_id bigint      not null references private_servers(private_server_id) on delete cascade,
    user_id           bigint      not null,
    created_at        timestamptz not null default now(),
    primary key (private_server_id, user_id)
);

create index if not exists idx_psw_user on private_server_whitelist(user_id);
