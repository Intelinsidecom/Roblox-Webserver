-- 0042_create_developer_product_assets.sql
-- Create separate table for developer product assets

-- Create developer product assets table
CREATE TABLE IF NOT EXISTS developer_product_assets (
    id BIGSERIAL PRIMARY KEY,
    developer_product_id BIGINT, -- Allow NULL initially, will be updated when product is created
    asset_name VARCHAR(255) NOT NULL,
    asset_type VARCHAR(100) DEFAULT 'Image' NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    content_hash VARCHAR(64) NOT NULL,
    thumbnail_url VARCHAR(512) NOT NULL,
    high_res_thumbnail_url VARCHAR(512),
    file_size BIGINT,
    mime_type VARCHAR(100),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by BIGINT NOT NULL,
    
    -- Foreign key constraints
    FOREIGN KEY (developer_product_id) REFERENCES developer_products(id) ON DELETE CASCADE,
    FOREIGN KEY (created_by) REFERENCES users(user_id) ON DELETE RESTRICT
);

-- Create index for faster lookups by developer product
CREATE INDEX IF NOT EXISTS idx_developer_product_assets_product_id ON developer_product_assets(developer_product_id);

-- Create index for content hash lookups
CREATE INDEX IF NOT EXISTS idx_developer_product_assets_hash ON developer_product_assets(content_hash);

-- Create index for creator
CREATE INDEX IF NOT EXISTS idx_developer_product_assets_created_by ON developer_product_assets(created_by);
