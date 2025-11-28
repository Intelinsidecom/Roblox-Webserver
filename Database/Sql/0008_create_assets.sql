-- 0008_create_assets.sql
-- Basic asset metadata table. Physical files are stored in the CDN/Assets directory keyed by content_hash.

create table if not exists assets (
    asset_id      bigserial primary key,
    name          text        not null,
    asset_type_id integer     not null,
    owner_user_id bigint      not null,
    content_hash  text        not null,
    file_extension text       null,
    content_type   text       null,
    thumbnail_url  text       null,
    created_at    timestamptz not null default now()
);

create index if not exists idx_assets_owner on assets(owner_user_id);
create index if not exists idx_assets_type on assets(asset_type_id);
create index if not exists idx_assets_hash on assets(content_hash);
