-- 0017_add_tix_and_membership_status.sql
-- Adds tix_balance and membership_status columns to users table.
-- membership_status: 0 = none, 1 = Builders Club, 2 = Turbo Builders Club, 3 = Outrageous Builders Club

alter table if exists users
    add column if not exists tix_balance bigint not null default 0;

alter table if exists users
    add column if not exists membership_status smallint not null default 0
        check (membership_status in (0,1,2,3));
