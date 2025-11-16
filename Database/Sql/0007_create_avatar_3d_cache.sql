CREATE TABLE IF NOT EXISTS avatar_3d_cache (
    config_hash  text PRIMARY KEY,
    model_hash   text NOT NULL,
    obj_file_name text NOT NULL,
    mtl_file_name text NOT NULL,
    width        integer NOT NULL,
    height       integer NOT NULL,
    created_at   timestamptz NOT NULL DEFAULT now()
);
