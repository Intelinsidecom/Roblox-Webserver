-- 0081_add_blocked_users.sql
-- Adds blocked users integer array to the users table

alter table users
  add column if not exists blocked  integer[] not null default '{}';
