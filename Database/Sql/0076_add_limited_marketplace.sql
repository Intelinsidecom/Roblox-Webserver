-- 0076_add_limited_marketplace.sql
-- Adds serial numbers, resale marketplace, RAP, and price history for limited items.

CREATE TABLE IF NOT EXISTS asset_serials (
    asset_id      BIGINT NOT NULL REFERENCES assets(asset_id),
    serial_number BIGINT NOT NULL,
    owner_user_id BIGINT NOT NULL REFERENCES users(user_id),
    assigned_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (asset_id, serial_number)
);

CREATE INDEX IF NOT EXISTS idx_asset_serials_owner ON asset_serials(owner_user_id);
CREATE UNIQUE INDEX IF NOT EXISTS idx_asset_serials_asset_owner ON asset_serials(asset_id, owner_user_id);

CREATE TABLE IF NOT EXISTS resale_listings (
    listing_id     BIGSERIAL PRIMARY KEY,
    asset_id       BIGINT NOT NULL REFERENCES assets(asset_id),
    seller_user_id BIGINT NOT NULL REFERENCES users(user_id),
    serial_number  BIGINT,
    price          BIGINT NOT NULL CHECK (price > 0),
    listed_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_resale_by_asset_price ON resale_listings(asset_id, price);
CREATE INDEX IF NOT EXISTS idx_resale_by_seller ON resale_listings(seller_user_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes WHERE indexname = 'idx_resale_one_per_serial'
    ) THEN
        CREATE UNIQUE INDEX idx_resale_one_per_serial
            ON resale_listings(asset_id, serial_number)
            WHERE serial_number IS NOT NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'assets' AND column_name = 'recent_average_price'
    ) THEN
        ALTER TABLE assets ADD COLUMN recent_average_price BIGINT DEFAULT 0;
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS price_history (
    id             BIGSERIAL PRIMARY KEY,
    asset_id       BIGINT NOT NULL REFERENCES assets(asset_id),
    price          BIGINT NOT NULL,
    recorded_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_price_history_asset_time
    ON price_history(asset_id, recorded_at DESC);
