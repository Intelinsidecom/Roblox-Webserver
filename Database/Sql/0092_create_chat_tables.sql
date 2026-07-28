-- Chat v2 tables for the AngularJS chat widget
-- Run this migration against your PostgreSQL database

CREATE TABLE IF NOT EXISTS conversations (
    id                BIGSERIAL PRIMARY KEY,
    title             TEXT DEFAULT '',
    conversation_type TEXT NOT NULL DEFAULT 'OneToOneConversation',
    creator_user_id   BIGINT NOT NULL,
    universe_id       BIGINT,
    is_deleted        BOOLEAN NOT NULL DEFAULT FALSE,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS conversation_participants (
    conversation_id       BIGINT NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    user_id               BIGINT NOT NULL,
    role                  TEXT NOT NULL DEFAULT 'Member',
    last_read_message_id  BIGINT DEFAULT 0,
    last_seen_at          TIMESTAMPTZ,
    is_notification_disabled BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (conversation_id, user_id)
);

CREATE TABLE IF NOT EXISTS chat_messages (
    id                BIGSERIAL PRIMARY KEY,
    conversation_id   BIGINT NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    sender_id         BIGINT NOT NULL,
    message_type      TEXT NOT NULL DEFAULT 'PlainText',
    content           TEXT NOT NULL DEFAULT '',
    event_type        TEXT,
    event_metadata    JSONB,
    is_deleted        BOOLEAN NOT NULL DEFAULT FALSE,
    sent_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_conv_participants_user ON conversation_participants(user_id);
CREATE INDEX IF NOT EXISTS idx_conv_participants_conv ON conversation_participants(conversation_id);
CREATE INDEX IF NOT EXISTS idx_chat_messages_conv ON chat_messages(conversation_id, id DESC);
CREATE INDEX IF NOT EXISTS idx_chat_messages_sender ON chat_messages(sender_id);
CREATE INDEX IF NOT EXISTS idx_conversations_creator ON conversations(creator_user_id);
