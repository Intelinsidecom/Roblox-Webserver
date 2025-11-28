-- 0009_create_user_assets.sql
-- Table mapping users to assets they own (inventory). For now used for T-Shirt assets.

create table if not exists user_assets (
    user_asset_id bigserial primary key,
    user_id       bigint      not null references users(user_id) on delete cascade,
    asset_id      bigint      not null references assets(asset_id) on delete cascade,
    created_at    timestamptz not null default now(),
    unique (user_id, asset_id)
);

create index if not exists idx_user_assets_user on user_assets(user_id);
create index if not exists idx_user_assets_asset on user_assets(asset_id);
