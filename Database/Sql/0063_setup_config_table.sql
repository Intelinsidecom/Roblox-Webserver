-- Setup Service Configuration Table Migration
-- Migration: 0002_setup_config_table.sql

CREATE TABLE IF NOT EXISTS setup (
    id SERIAL PRIMARY KEY,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE setup 
ADD COLUMN IF NOT EXISTS current_windowsplayer_version VARCHAR(36),
ADD COLUMN IF NOT EXISTS current_rcc_version VARCHAR(36),
ADD COLUMN IF NOT EXISTS current_studio_version VARCHAR(36);

INSERT INTO setup (current_windowsplayer_version, current_rcc_version, current_studio_version) 
VALUES (
    '5541c7b5a06c39b267a5efae6628e003',
    '5541c7b5a06c39b267a5efae6628e003',
    '5541c7b5a06c39b267a5efae6628e003'
) ON CONFLICT DO NOTHING;

CREATE OR REPLACE FUNCTION get_client_version(client_type TEXT)
RETURNS TEXT AS $$
BEGIN
    RETURN CASE 
        WHEN client_type = 'WindowsPlayer' THEN
            (SELECT current_windowsplayer_version FROM setup ORDER BY id DESC LIMIT 1)
        WHEN client_type = 'RCC' THEN
            (SELECT current_rcc_version FROM setup ORDER BY id DESC LIMIT 1)
        WHEN client_type = 'Studio' THEN
            (SELECT current_studio_version FROM setup ORDER BY id DESC LIMIT 1)
        ELSE
            NULL
    END;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_client_version(client_type TEXT, new_version TEXT)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE setup 
    SET 
        current_windowsplayer_version = CASE WHEN client_type = 'WindowsPlayer' THEN new_version ELSE current_windowsplayer_version END,
        current_rcc_version = CASE WHEN client_type = 'RCC' THEN new_version ELSE current_rcc_version END,
        current_studio_version = CASE WHEN client_type = 'Studio' THEN new_version ELSE current_studio_version END,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = (SELECT id FROM setup ORDER BY id DESC LIMIT 1);
    
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;


