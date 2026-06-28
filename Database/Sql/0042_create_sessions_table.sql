-- 0042_create_sessions_table.sql
-- Creates the sessions table used by TokenService for JWT session persistence.
-- Required for login flow — CreateJwtSessionAsync and ValidateSessionAsync both reference this table.

create table if not exists sessions (
    token       text            primary key,
    user_id     bigint          not null references users(user_id),
    created_at  timestamptz     not null default now(),
    expires_at  timestamptz     not null,
    last_ip     inet            null
);

create index if not exists idx_sessions_user_id on sessions (user_id);
create index if not exists idx_sessions_expires_at on sessions (expires_at);
