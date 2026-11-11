CREATE TABLE IF NOT EXISTS website_settings (
    global_message TEXT DEFAULT NULL,
    lockdown_mode_enabled BOOLEAN DEFAULT FALSE,
    lockdown_mode_reason TEXT DEFAULT NULL
);

INSERT INTO website_settings (global_message, lockdown_mode_enabled, lockdown_mode_reason)
VALUES (NULL, FALSE, NULL);