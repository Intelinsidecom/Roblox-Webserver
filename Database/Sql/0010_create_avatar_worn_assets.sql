-- 0010_create_avatar_worn_assets.sql
-- Stores which assets a user is currently wearing on their avatar.

create table if not exists avatar_worn_assets (
    user_id    bigint not null references users(user_id) on delete cascade,
    asset_id   bigint not null references assets(asset_id) on delete cascade,
    created_at timestamptz not null default now(),
    primary key (user_id, asset_id)
);

create index if not exists idx_avatar_worn_assets_user on avatar_worn_assets(user_id);
create index if not exists idx_avatar_worn_assets_asset on avatar_worn_assets(asset_id);
