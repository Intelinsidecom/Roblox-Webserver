-- 0077_create_notifications.sql
-- Adds the notifications table for the notification stream.

CREATE TABLE IF NOT EXISTS notifications (
    id                      BIGSERIAL PRIMARY KEY,
    user_id                 BIGINT NOT NULL REFERENCES users(user_id),
    notification_source_type VARCHAR(64) NOT NULL,
    sender_user_id          BIGINT,
    sender_user_name        VARCHAR(255) DEFAULT '',
    subject_type            VARCHAR(64) DEFAULT '',
    subject_id              BIGINT DEFAULT 0,
    subject_name            VARCHAR(255) DEFAULT '',
    is_read                 BOOLEAN DEFAULT FALSE,
    is_interacted           BOOLEAN DEFAULT FALSE,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_user_unread ON notifications(user_id, is_read) WHERE is_read = FALSE;
CREATE INDEX IF NOT EXISTS idx_notifications_created_at ON notifications(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_notifications_user_created ON notifications(user_id, created_at DESC);
