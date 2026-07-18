-- 0075_create_asset_sales_log.sql
-- Tracks individual asset purchases so we can query sales over time windows (e.g. last 7 days).

CREATE TABLE IF NOT EXISTS asset_sales_log (
    id              BIGSERIAL PRIMARY KEY,
    asset_id        BIGINT NOT NULL,
    buyer_user_id   BIGINT NOT NULL,
    price           BIGINT NOT NULL,
    currency        SMALLINT NOT NULL DEFAULT 1,
    sold_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_asset_sales_log_asset_sold
    ON asset_sales_log (asset_id, sold_at);

CREATE INDEX IF NOT EXISTS idx_asset_sales_log_sold_at
    ON asset_sales_log (sold_at);
