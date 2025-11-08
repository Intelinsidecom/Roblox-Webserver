-- 0003_add_thumbnail_url.sql
-- Adds a column to store the full URL to the user's latest avatar/headshot thumbnail.

alter table if exists users
    add column if not exists thumbnail_url text;
