-- 0040_create_authentication_tickets.sql
-- Creates the authentication_tickets table for Windows client game authentication

create table if not exists authentication_tickets (
    ticket_id                bigint primary key generated always as identity,
    ticket_token             text            not null unique,
    user_id                  bigint          not null references users(user_id) on delete cascade,
    place_id                 bigint          null references assets(asset_id) on delete cascade,
    universe_id              bigint          null references universes(universe_id) on delete cascade,
    authentication_url       text            not null,
    join_script_url          text            not null,
    browser_tracker_id       text            null,
    created_at               timestamptz     not null default now(),
    expires_at               timestamptz     not null default (now() + interval '15 minutes'),
    used_at                  timestamptz     null,
    client_ip                inet            null,
    client_user_agent        text            null,
    is_active                boolean         not null default true,
    ticket_type              text            not null default 'game_session', -- game_session, teleport, etc.
    
    constraint chk_authentication_tickets_ticket_type check (ticket_type in ('game_session', 'teleport', 'api_request'))
);

create index if not exists idx_authentication_tickets_token on authentication_tickets (ticket_token);
create index if not exists idx_authentication_tickets_user_id on authentication_tickets (user_id);
create index if not exists idx_authentication_tickets_place_id on authentication_tickets (place_id);
create index if not exists idx_authentication_tickets_active_expires on authentication_tickets (is_active, expires_at);

create or replace function cleanup_expired_authentication_tickets()
returns void as $$
begin
    update authentication_tickets 
    set is_active = false 
    where is_active = true and expires_at < now();
end;
$$ language plpgsql;

create index if not exists idx_authentication_tickets_expires_at on authentication_tickets (expires_at);
