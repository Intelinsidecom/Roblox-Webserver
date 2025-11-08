-- 0002_create_bodycolors.sql
-- Create bodycolors table and ensure each user has a default row. Also add trigger for new users.

create table if not exists bodycolors (
    user_id         bigint      primary key references users(user_id) on delete cascade,
    head_color      integer     not null default 1,
    left_arm_color  integer     not null default 1,
    left_leg_color  integer     not null default 1,
    right_arm_color integer     not null default 1,
    right_leg_color integer     not null default 1,
    torso_color     integer     not null default 1
);

-- Backfill for existing users
insert into bodycolors (user_id)
select u.user_id
from users u
left join bodycolors bc on bc.user_id = u.user_id
where bc.user_id is null;

-- Trigger to auto-create defaults on new users
create or replace function trg_ins_users_bodycolors()
returns trigger as $$
begin
    insert into bodycolors (user_id)
    values (new.user_id)
    on conflict (user_id) do nothing;
    return new;
end;
$$ language plpgsql;

-- Create trigger if not exists (Postgres doesn't support IF NOT EXISTS for triggers directly)
-- Drop any existing trigger with the same name to be idempotent
do $$
begin
    if exists (
        select 1 from pg_trigger t
        join pg_class c on c.oid = t.tgrelid
        where t.tgname = 'trg_users_bodycolors_ins' and c.relname = 'users'
    ) then
        execute 'drop trigger trg_users_bodycolors_ins on users';
    end if;
    execute 'create trigger trg_users_bodycolors_ins after insert on users for each row execute function trg_ins_users_bodycolors()';
end
$$;
