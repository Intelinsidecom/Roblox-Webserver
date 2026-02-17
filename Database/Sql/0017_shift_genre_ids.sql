-- 0017_shift_genre_ids.sql
-- Shifts all existing genre IDs by +1 to fix the genre filtering issue
-- This ensures that "All" genre is now ID 1 instead of 0, and all other genres are shifted accordingly

UPDATE assets 
SET genre = genre + 1 
WHERE genre > 0;

UPDATE assets 
SET genre = 1 
WHERE genre = 0;

SELECT 
    genre,
    COUNT(*) as asset_count,
    CASE 
        WHEN genre = 1 THEN 'All'
        WHEN genre = 2 THEN 'Town & City'
        WHEN genre = 3 THEN 'Fantasy'
        WHEN genre = 4 THEN 'Sci-Fi'
        WHEN genre = 5 THEN 'Ninja'
        WHEN genre = 6 THEN 'Scary'
        WHEN genre = 7 THEN 'Pirate'
        WHEN genre = 8 THEN 'Adventure'
        WHEN genre = 9 THEN 'Sports'
        WHEN genre = 10 THEN 'Funny'
        WHEN genre = 11 THEN 'Wild West'
        WHEN genre = 12 THEN 'War'
        WHEN genre = 13 THEN 'Skate Park'
        WHEN genre = 14 THEN 'Tutorial'
        WHEN genre = 15 THEN 'RPG'
        WHEN genre = 16 THEN 'FPS'
        WHEN genre = 17 THEN 'Fighting'
        WHEN genre = 18 THEN 'Building'
        WHEN genre = 19 THEN 'Military'
        WHEN genre = 20 THEN 'Naval'
        WHEN genre = 21 THEN 'Medieval'
        ELSE 'Unknown'
    END as genre_name
FROM assets 
GROUP BY genre 
ORDER BY genre;
