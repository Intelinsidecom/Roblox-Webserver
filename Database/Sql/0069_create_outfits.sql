-- 0069_create_outfits.sql
-- Creates the outfits table and adds owned_outfits jsonb array to users.

create table if not exists outfits (
    outfit_id      bigserial    primary key,
    user_id        bigint       not null references users(user_id) on delete cascade,
    name           text         not null,
    body_colors    jsonb        not null default '{}'::jsonb,
    asset_ids      jsonb        not null default '[]'::jsonb,
    thumbnail_url  text         null,
    created_at     timestamptz  not null default now(),
    updated_at     timestamptz  not null default now()
);

create index if not exists idx_outfits_user_id on outfits (user_id);
create index if not exists idx_outfits_created_at on outfits (created_at desc);

alter table if exists users
    add column if not exists owned_outfits jsonb not null default '[]'::jsonb;

create index if not exists idx_users_owned_outfits on users using gin (owned_outfits);
