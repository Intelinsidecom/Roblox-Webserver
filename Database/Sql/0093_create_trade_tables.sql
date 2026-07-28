-- 0093_create_trade_tables.sql
-- Adds tables for the trading system (limiteds, BC only).

CREATE TABLE IF NOT EXISTS trades (
    id              BIGSERIAL PRIMARY KEY,
    sender_id       BIGINT NOT NULL REFERENCES users(user_id),
    receiver_id     BIGINT NOT NULL REFERENCES users(user_id),
    status          TEXT NOT NULL DEFAULT 'Open',
    sender_robux    BIGINT NOT NULL DEFAULT 0,
    receiver_robux  BIGINT NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    expires_at      TIMESTAMPTZ NOT NULL DEFAULT (now() + interval '7 days'),
    counter_of_id   BIGINT REFERENCES trades(id)
);

CREATE INDEX IF NOT EXISTS idx_trades_sender ON trades(sender_id, status);
CREATE INDEX IF NOT EXISTS idx_trades_receiver ON trades(receiver_id, status);
CREATE INDEX IF NOT EXISTS idx_trades_status ON trades(status);

CREATE TABLE IF NOT EXISTS trade_items (
    id              BIGSERIAL PRIMARY KEY,
    trade_id        BIGINT NOT NULL REFERENCES trades(id) ON DELETE CASCADE,
    user_asset_id   BIGINT NOT NULL,
    asset_id        BIGINT NOT NULL REFERENCES assets(asset_id),
    agent_id        BIGINT NOT NULL REFERENCES users(user_id),
    side            TEXT NOT NULL CHECK (side IN ('offer', 'request')),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_trade_items_trade ON trade_items(trade_id);
CREATE INDEX IF NOT EXISTS idx_trade_items_agent ON trade_items(agent_id);
CREATE INDEX IF NOT EXISTS idx_trade_items_user_asset ON trade_items(user_asset_id);

CREATE TABLE IF NOT EXISTS trade_history (
    id              BIGSERIAL PRIMARY KEY,
    trade_id        BIGINT NOT NULL REFERENCES trades(id) ON DELETE CASCADE,
    action          TEXT NOT NULL,
    actor_id        BIGINT REFERENCES users(user_id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_trade_history_trade ON trade_history(trade_id);
