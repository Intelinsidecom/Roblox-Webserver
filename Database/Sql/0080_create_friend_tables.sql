-- 0080_create_friend_tables.sql
-- Creates tables for the friends system: user_friends, friend_requests
-- Adds follower/following integer arrays to the users table

create table if not exists user_friends (
    user_id         bigint not null references users(user_id),
    friend_user_id  bigint not null references users(user_id),
    created_at      timestamptz not null default now(),
    primary key (user_id, friend_user_id)
);

create index if not exists idx_user_friends_user on user_friends (user_id);
create index if not exists idx_user_friends_friend on user_friends (friend_user_id);

create table if not exists friend_requests (
    id           bigserial primary key,
    sender_id    bigint not null references users(user_id),
    receiver_id  bigint not null references users(user_id),
    status       text not null default 'pending'
                 constraint chk_friend_request_status check (status in ('pending', 'accepted', 'declined')),
    created_at   timestamptz not null default now(),
    updated_at   timestamptz
);

create index if not exists idx_friend_requests_receiver on friend_requests (receiver_id, status);
create index if not exists idx_friend_requests_sender on friend_requests (sender_id, status);

alter table users
  add column if not exists followers  integer[] not null default '{}',
  add column if not exists following  integer[] not null default '{}';
