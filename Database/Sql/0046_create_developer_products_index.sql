-- 0041_create_developer_products_index.sql
-- Add index for faster lookups by universe

-- Create index for faster lookups by universe
CREATE INDEX IF NOT EXISTS idx_developer_products_universe_id ON developer_products(universe_id);
