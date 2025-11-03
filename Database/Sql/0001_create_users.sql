-- 0001_create_users.sql
-- Creates the users table and required columns. Some list-like properties are stored as JSONB.

-- Create enum type for gender if it doesn't exist
do $$
begin
    if not exists (select 1 from pg_type where typname = 'gender_enum') then
        create type gender_enum as enum ('none','female','male');
    end if;
end$$;

 create table if not exists users (
    user_id                  bigint primary key,
    user_name                text            not null,
    previous_user_names      jsonb           not null default '[]'::jsonb,
    moderation_status        text            not null,
    premium_member           boolean         not null default false,
    subscription_type        text            null,
    subscription_expiration_date text       null,
    subscription_renewal_date   text       null,
    loyal_since              text            null,
    current_location         text            null,
    xbox_user                boolean         not null default false,
    email                    text            null,
    phone_number             text            null,
    password                 text            null,
    birthday                 date            null,
    gender                   gender_enum     not null default 'none',
    "2sv_enabled"            boolean         not null default false,
    "2sv_verification_types" jsonb           not null default '[]'::jsonb,
    user_created             timestamptz     null,
    -- Alias of user_created for convenience (generated column)
    join_date                timestamptz     generated always as (user_created) stored,
    last_location            text            null,
    last_activity            timestamptz     null,
    role_set                 text            null,
    -- Commerce / activity
    last_payment_at          timestamptz     null,
    last_purchase_at         timestamptz     null,
    robux_balance            bigint          not null default 0,
    -- Social counts
    badges_count             integer         not null default 0,
    friends_count            integer         not null default 0,
    followers_count          integer         not null default 0,
    following_count          integer         not null default 0,
    favorite_games_count     integer         not null default 0,
    favorite_items_count     integer         not null default 0,
    blocked_users            integer         not null default 0,
    -- Presence / session
    presence_universe_id     bigint          null,
    presence_place_id        bigint          null,
    last_seen_game_id        bigint          null,
    last_seen_universe_id    bigint          null,
    last_login_ip            inet            null,
    last_login_at            timestamptz     null,
    -- Security / verification
    email_verified           boolean         not null default false,
    phone_verified           boolean         not null default false,
    account_restrictions_enabled boolean     not null default false,
    "2sv_recovery_codes_set" boolean        not null default false,
    password_last_changed_at timestamptz     null,
    -- Profile
    description_bio          text            null,
    avatar_thumbnail_url     text            null,
    headshot_thumbnail_url   text            null,
    profile_visibility       text            not null default 'public',
    inventory_privacy        text            not null default 'public',
    -- Communication capabilities
    can_pm                   boolean         not null default false,
    can_chat                 boolean         not null default false,
    can_trade                boolean         not null default false,
    chat_privacy_level       text            null,
    -- Developer & analytics
    is_developer             boolean         not null default false,
    account_age_days         integer         not null default 0,
    locale                   text            null,
    language                 text            null,
    country_iso              text            null,
    ,constraint chk_users_profile_visibility check (profile_visibility in ('public','friends','private'))
    ,constraint chk_users_inventory_privacy check (inventory_privacy in ('public','friends','private'))
    ,constraint chk_users_country_iso_len check (country_iso is null or char_length(country_iso) = 2)
 );

-- Helpful indexes
create index if not exists idx_users_user_name on users (user_name);
create index if not exists idx_users_email on users (email);
create index if not exists idx_users_last_activity on users (last_activity);
