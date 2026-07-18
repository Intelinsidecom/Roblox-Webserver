-- 0078_add_user_status_text.sql
-- Adds the status_text column for the profile status (the "What are you up to?" blurb).

ALTER TABLE users ADD COLUMN IF NOT EXISTS status_text text null;
