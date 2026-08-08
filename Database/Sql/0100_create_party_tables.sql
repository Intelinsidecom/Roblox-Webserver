-- Party tables for the chat v2 widget (used by /v1.0/party/* endpoints)
-- Run this migration against your PostgreSQL database

CREATE TABLE IF NOT EXISTS parties (
    id              BIGSERIAL PRIMARY KEY,
    conversation_id BIGINT NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    leader_user_id  BIGINT NOT NULL,
    game_id         BIGINT,
    game_place_id   BIGINT,
    is_deleted      BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS party_members (
    party_id      BIGINT NOT NULL REFERENCES parties(id) ON DELETE CASCADE,
    user_id       BIGINT NOT NULL,
    member_status TEXT NOT NULL DEFAULT 'Invited',
    PRIMARY KEY (party_id, user_id)
);

CREATE INDEX IF NOT EXISTS idx_parties_conversation ON parties(conversation_id);
CREATE INDEX IF NOT EXISTS idx_parties_leader ON parties(leader_user_id);
CREATE INDEX IF NOT EXISTS idx_party_members_user ON party_members(user_id);
CREATE INDEX IF NOT EXISTS idx_party_members_party ON party_members(party_id);
