-- 0091_create_private_messages.sql
-- Adds the private_messages table for the messaging system.

CREATE TABLE IF NOT EXISTS private_messages (
    id                BIGSERIAL PRIMARY KEY,
    sender_id         BIGINT NOT NULL REFERENCES users(user_id),
    recipient_id      BIGINT NOT NULL REFERENCES users(user_id),
    subject           TEXT NOT NULL DEFAULT '',
    body              TEXT NOT NULL DEFAULT '',
    is_read           BOOLEAN NOT NULL DEFAULT FALSE,
    is_archived       BOOLEAN NOT NULL DEFAULT FALSE,
    is_system_message BOOLEAN NOT NULL DEFAULT FALSE,
    reply_to_id       BIGINT REFERENCES private_messages(id),
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_pm_recipient_inbox ON private_messages(recipient_id, created_at DESC) WHERE is_archived = FALSE;
CREATE INDEX IF NOT EXISTS idx_pm_recipient_archive ON private_messages(recipient_id, created_at DESC) WHERE is_archived = TRUE;
CREATE INDEX IF NOT EXISTS idx_pm_sender ON private_messages(sender_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_pm_recipient_unread ON private_messages(recipient_id, is_read) WHERE is_read = FALSE;
