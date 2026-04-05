-- 0040_create_game_presence.sql
-- Creates game_presence table for tracking active player sessions

create table if not exists game_presence (
    uid bigint primary key,
    placeid bigint not null,
    jobid text not null,
    created_at timestamptz not null default NOW(),
    updated_at timestamptz not null default NOW(),
    last_ping timestamptz not null default NOW()
);

create index if not exists idx_game_presence_placeid on game_presence (placeid);
create index if not exists idx_game_presence_jobid on game_presence (jobid);
create index if not exists idx_game_presence_updated_at on game_presence (updated_at);
create index if not exists idx_game_presence_last_ping on game_presence (last_ping);

alter table game_presence add constraint fk_game_presence_place 
    foreign key (placeid) references assets(asset_id) on delete cascade;
