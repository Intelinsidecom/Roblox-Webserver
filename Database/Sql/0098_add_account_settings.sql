-- 0098_add_account_settings.sql
-- Account settings storage.

alter table users
    add column if not exists app_chat_privacy              text not null default 'Friends'
    constraint chk_users_app_chat_privacy
        check (app_chat_privacy in ('Friends','NoOne'));

alter table users
    add column if not exists game_chat_privacy             text not null default 'AllUsers'
    constraint chk_users_game_chat_privacy
        check (game_chat_privacy in ('AllUsers','NoOne'));

alter table users
    add column if not exists private_message_privacy      text not null default 'Friends'
    constraint chk_users_private_message_privacy
        check (private_message_privacy in ('All','Followers','Following','Friends','NoOne'));

alter table users
    add column if not exists private_server_invite_privacy text not null default 'Friends'
    constraint chk_users_private_server_invite_privacy
        check (private_server_invite_privacy in ('All','Followers','Following','Friends','NoOne'));

alter table users
    add column if not exists follow_me_privacy            text not null default 'Friends'
    constraint chk_users_follow_me_privacy
        check (follow_me_privacy in ('All','Followers','Following','Friends','NoOne'));

alter table users
    add column if not exists trade_privacy                text not null default 'Friends'
    constraint chk_users_trade_privacy
        check (trade_privacy in ('All','Followers','Following','Friends','NoOne'));

alter table users
    add column if not exists trade_value                  smallint not null default 0
    constraint chk_users_trade_value check (trade_value between 0 and 3);

alter table users
    add column if not exists account_pin_enabled            boolean not null default false;
alter table users
    add column if not exists account_pin_unlocked_until     bigint  not null default 0;
alter table users
    add column if not exists account_pin_hash               text    null;
alter table users
    add column if not exists account_pin_salt               text    null;

alter table users
    add column if not exists social_facebook_url            text    null;
alter table users
    add column if not exists social_twitter_url             text    null;
alter table users
    add column if not exists social_googleplus_url          text    null;
alter table users
    add column if not exists social_youtube_url             text    null;
alter table users
    add column if not exists social_twitch_url              text    null;
alter table users
    add column if not exists social_networks_visibility     smallint not null default 6;

alter table users
    add column if not exists receive_newsletter             boolean not null default false;

alter table users
    add column if not exists opted_out_recv_dst_types       text[]  not null default '{}';
alter table users
    add column if not exists notification_bands             jsonb  not null default '[]'::jsonb;

alter table users
    add column if not exists account_settings              jsonb   not null default '{}'::jsonb;

create index if not exists idx_users_account_pin_unlocked_until
    on users (account_pin_unlocked_until)
    where account_pin_enabled;
