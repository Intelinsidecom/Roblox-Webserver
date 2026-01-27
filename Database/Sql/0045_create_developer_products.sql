-- 0040_create_developer_products.sql
-- Create developer products table for place monetization

-- Create developer products table first (minimal version)
CREATE TABLE IF NOT EXISTS developer_products (
    id BIGSERIAL PRIMARY KEY,
    universe_id BIGINT NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price_in_robux INTEGER NOT NULL CHECK (price_in_robux >= 0),
    price_in_tix INTEGER NOT NULL CHECK (price_in_tix >= 0),
    image_asset_id BIGINT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);