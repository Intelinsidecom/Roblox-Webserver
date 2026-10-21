-- 0106_unique_usernames.sql
-- clean up existing duplicates.


begin;

with dupes as (
    select u.user_id,
           (select min(k.user_id) from users k
             where lower(k.user_name) = lower(u.user_name)) as keep_id
      from users u
     where u.user_id <> (select min(k.user_id) from users k
                          where lower(k.user_name) = lower(u.user_name))
)
update assets a
   set owner_user_id = d.keep_id
  from dupes d
 where a.owner_user_id = d.user_id;

with dupes as (
    select u.user_id
      from users u
     where u.user_id <> (select min(k.user_id) from users k
                          where lower(k.user_name) = lower(u.user_name))
)
delete from friend_requests fr
 using dupes d
 where (fr.sender_id = d.user_id or fr.receiver_id = d.user_id);

with dupes as (
    select u.user_id
      from users u
     where u.user_id <> (select min(k.user_id) from users k
                          where lower(k.user_name) = lower(u.user_name))
)
delete from notifications n
 using dupes d
 where n.user_id = d.user_id;

with dupes as (
    select u.user_id
      from users u
     where u.user_id <> (select min(k.user_id) from users k
                          where lower(k.user_name) = lower(u.user_name))
)
delete from user_friends uf
 using dupes d
 where uf.user_id = d.user_id or uf.friend_user_id = d.user_id;

with dupes as (
    select u.user_id
      from users u
     where u.user_id <> (select min(k.user_id) from users k
                          where lower(k.user_name) = lower(u.user_name))
)
delete from feed_entries fe
 using dupes d
 where fe.user_id = d.user_id;

delete from users u
 where u.user_id <> (select min(k.user_id) from users k
                      where lower(k.user_name) = lower(u.user_name));

drop index if exists idx_users_user_name;
create unique index idx_users_user_name_lower on users (lower(user_name));

commit;
