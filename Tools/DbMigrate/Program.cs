using System.Reflection;
using System.Collections.Generic;
using DbUp;
using Microsoft.Extensions.Configuration;
using Npgsql;

internal class Program
{
    private static int Main(string[] args)
    {
        // Args: --connection <connString> [--env Development|Production]
        string? connArg = null;
        string environment = "Development";
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--connection" && i + 1 < args.Length)
            {
                connArg = args[++i];
            }
            else if (args[i] == "--env" && i + 1 < args.Length)
            {
                environment = args[++i];
            }
        }

        // Resolve solution root (two levels up from Tools/DbMigrate/bin/...)
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var projectDir = Path.GetFullPath(Path.Combine(exeDir, "..", "..", ".."));
        var repoRoot = Path.GetFullPath(Path.Combine(projectDir, "..", ".."));

        // Load connection from this tool's appsettings.{env}.json if not provided
        string? connectionString = connArg;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var toolDir = projectDir; // Tools/DbMigrate
            var config = new ConfigurationBuilder()
                .SetBasePath(toolDir)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            connectionString = config.GetConnectionString("Default");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine("Connection string not found. Provide --connection or configure Tools/DbMigrate/appsettings*.json (ConnectionStrings:Default).");
            return 2;
        }

        // Scripts folder: <repoRoot>/Database/Sql
        var scriptsPath = Path.Combine(repoRoot, "Database", "Sql");
        if (!Directory.Exists(scriptsPath))
        {
            Console.Error.WriteLine($"Scripts folder not found: {scriptsPath}");
            return 3;
        }

        Console.WriteLine("Select an action:");
        Console.WriteLine("1) Migrate/Update database");
        Console.WriteLine("2) List tables");
        Console.WriteLine("3) WIPE database (DROP ALL TABLES) - DANGEROUS");
        Console.WriteLine("4) Describe users table (columns)");
        Console.WriteLine("5) List users (id, name, created)");
        Console.WriteLine("6) View user details (by id or name)");
        Console.WriteLine("7) Asset maintenance (wipe all assets or a single asset)");
        Console.WriteLine("8) Add Robux/Tix to a user");
        Console.WriteLine("9) Thumbnail maintenance (delete thumbnails from CDN and database)");
        Console.Write("Enter choice (1-9): ");
        var key = Console.ReadKey(intercept: true).KeyChar;
        Console.WriteLine();

        if (key == '1')
        {
            // Ensure DB exists and run migrations (journal table: schemaversions)
            try
            {
                EnsureDatabase.For.PostgresqlDatabase(connectionString);

                var upgrader = DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(scriptsPath)
                    .LogToConsole()
                    .Build();

                var result = upgrader.PerformUpgrade();
                if (!result.Successful)
                {
                    Console.Error.WriteLine(result.Error);
                    return -1;
                }

                Console.WriteLine("Success! Database is up to date.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        else if (key == '2')
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                const string sql = @"select table_schema, table_name
                                      from information_schema.tables
                                      where table_type='BASE TABLE'
                                        and table_schema not in ('pg_catalog','information_schema')
                                      order by table_schema, table_name";
                using var cmd = new NpgsqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int count = 0;
                Console.WriteLine("Tables:");
                while (reader.Read())
                {
                    count++;
                    var schema = reader.GetString(0);
                    var name = reader.GetString(1);
                    Console.WriteLine($"- {schema}.{name}");
                }
                if (count == 0)
                {
                    Console.WriteLine("(no tables)");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        else if (key == '4')
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                const string sql = @"select column_name, data_type, is_nullable, column_default
                                      from information_schema.columns
                                      where table_schema='public' and table_name='users'
                                      order by ordinal_position";
                using var cmd = new NpgsqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int count = 0;
                Console.WriteLine("public.users columns:");
                while (reader.Read())
                {
                    count++;
                    var name = reader.GetString(0);
                    var type = reader.GetString(1);
                    var nullable = reader.GetString(2);
                    var def = reader.IsDBNull(3) ? null : reader.GetString(3);
                    Console.WriteLine($"- {name} :: {type} {(nullable=="YES"?"NULL":"NOT NULL")} {(string.IsNullOrEmpty(def)?"":"default "+def)}");
                }
                if (count == 0)
                {
                    Console.WriteLine("(table not found or has no columns)");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        else if (key == '5')
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                const string sql = @"select user_id, user_name, user_created from users order by user_id limit 100";
                using var cmd = new NpgsqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int count = 0;
                Console.WriteLine("Users:");
                while (reader.Read())
                {
                    count++;
                    var id = reader.IsDBNull(0) ? (long?)null : reader.GetInt64(0);
                    var name = reader.IsDBNull(1) ? null : reader.GetString(1);
                    var created = reader.IsDBNull(2) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(2);
                    Console.WriteLine($"- {id} | {name} | {created}");
                }
                if (count == 0)
                {
                    Console.WriteLine("(no users)");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        else if (key == '6')
        {
            try
            {
                Console.Write("Enter user id or user name: ");
                var input = Console.ReadLine();
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                bool byId = long.TryParse(input, out var id);
                var sql = byId ? "select * from users where user_id = @v limit 1" : "select * from users where user_name = @v limit 1";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("v", byId ? (object)id : (object)(input ?? string.Empty));
                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    Console.WriteLine("(user not found)");
                    return 0;
                }
                int fieldCount = reader.FieldCount;
                Console.WriteLine("User row:");
                for (int i = 0; i < fieldCount; i++)
                {
                    var col = reader.GetName(i);
                    object valObj = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    string val = valObj == null ? "NULL" : Convert.ToString(valObj) ?? string.Empty;
                    Console.WriteLine($"- {col}: {val}");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        else if (key == '3')
        {
            // Destructive wipe: drop and recreate public schema
            Console.WriteLine("\nWARNING: You are about to DROP ALL OBJECTS in this database (schema 'public').");
            Console.WriteLine("This operation is IRREVERSIBLE and will DELETE ALL TABLES, VIEWS, FUNCTIONS, DATA.");
            Console.WriteLine($"Connection: {connectionString}");
            Console.WriteLine();
            Console.Write("Type 'WIPE' to continue (or anything else to cancel): ");
            var confirm1 = Console.ReadLine();
            if (!string.Equals(confirm1, "WIPE", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Cancelled.");
                return 1;
            }
            Console.Write("Type the target database name to confirm: ");
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var expectedDb = builder.Database;
            var confirmDb = Console.ReadLine();
            if (!string.Equals(confirmDb, expectedDb, StringComparison.Ordinal))
            {
                Console.WriteLine("Database name did not match. Cancelled.");
                return 1;
            }
            Console.Write("FINAL CONFIRMATION: Type 'DROP' to proceed: ");
            var confirm2 = Console.ReadLine();
            if (!string.Equals(confirm2, "DROP", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Cancelled.");
                return 1;
            }

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                using var tx = conn.BeginTransaction();
                using (var cmd = new NpgsqlCommand("DROP SCHEMA IF EXISTS public CASCADE;", conn, tx))
                    cmd.ExecuteNonQuery();
                using (var cmd = new NpgsqlCommand("CREATE SCHEMA public;", conn, tx))
                    cmd.ExecuteNonQuery();
                // Restore common privileges
                using (var cmd = new NpgsqlCommand("GRANT ALL ON SCHEMA public TO postgres;", conn, tx))
                    cmd.ExecuteNonQuery();
                using (var cmd = new NpgsqlCommand("GRANT ALL ON SCHEMA public TO public;", conn, tx))
                    cmd.ExecuteNonQuery();
                tx.Commit();
                Console.WriteLine("Database schema 'public' has been wiped and recreated.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        else if (key == '7')
        {
            Console.WriteLine("Asset maintenance:");
            Console.WriteLine("1) Wipe ALL assets from database and delete their files");
            Console.WriteLine("2) Wipe a single asset (and linked T-Shirt image/XML) from database and delete files");
            Console.Write("Enter choice (1-2): ");
            var subKey = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            var websiteDir = Path.Combine(repoRoot, "Website");
            string? assetsDirectory = null;
            if (Directory.Exists(websiteDir))
            {
                var cfg = new ConfigurationBuilder()
                    .SetBasePath(websiteDir)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
                assetsDirectory = cfg["Assets:Directory"];
            }

            if (string.IsNullOrWhiteSpace(assetsDirectory))
            {
                Console.WriteLine("Assets directory not configured in Website/appsettings*.json; file deletions will be skipped.");
            }

            string? assetFolder = null;
            if (!string.IsNullOrWhiteSpace(assetsDirectory))
            {
                assetFolder = Path.Combine(assetsDirectory, "asset");
            }

            if (subKey == '1')
            {
                Console.WriteLine();
                Console.WriteLine("WARNING: This will DELETE ALL rows from assets and user_assets, and attempt to delete all asset files.");
                Console.Write("Type 'WIPE_ASSETS' to continue (or anything else to cancel): ");
                var confirm = Console.ReadLine();
                if (!string.Equals(confirm, "WIPE_ASSETS", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Cancelled.");
                    return 1;
                }

                try
                {
                    using var conn = new NpgsqlConnection(connectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    var fileInfo = new List<(string Hash, string? Ext)>();
                    const string selectSql = "select content_hash, file_extension from assets";
                    using (var cmdSelect = new NpgsqlCommand(selectSql, conn, tx))
                    using (var reader = cmdSelect.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var hash = reader.GetString(0);
                            var ext = reader.IsDBNull(1) ? null : reader.GetString(1);
                            fileInfo.Add((hash, ext));
                        }
                    }

                    using (var cmdDelUserAssets = new NpgsqlCommand("delete from user_assets;", conn, tx))
                    {
                        cmdDelUserAssets.ExecuteNonQuery();
                    }

                    using (var cmdDelAssets = new NpgsqlCommand("delete from assets;", conn, tx))
                    {
                        cmdDelAssets.ExecuteNonQuery();
                    }

                    tx.Commit();

                    if (!string.IsNullOrWhiteSpace(assetFolder) && Directory.Exists(assetFolder))
                    {
                        foreach (var (hash, ext) in fileInfo)
                        {
                            var fileName = hash + (ext ?? string.Empty);
                            var path = Path.Combine(assetFolder, fileName);
                            if (File.Exists(path))
                            {
                                try
                                {
                                    File.Delete(path);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }

                    Console.WriteLine("All assets and user_assets rows have been deleted. Associated files were deleted where possible.");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    return -1;
                }
            }
            else if (subKey == '2')
            {
                Console.Write("Enter asset id: ");
                var input = Console.ReadLine();
                if (!long.TryParse(input, out var rootAssetId) || rootAssetId <= 0)
                {
                    Console.WriteLine("Invalid asset id.");
                    return 1;
                }

                try
                {
                    using var conn = new NpgsqlConnection(connectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    const string baseSql = "select asset_id, asset_type_id, name, owner_user_id, content_hash, file_extension from assets where asset_id = @id limit 1";
                    using var cmdBase = new NpgsqlCommand(baseSql, conn, tx);
                    cmdBase.Parameters.AddWithValue("id", rootAssetId);

                    long assetId;
                    int assetTypeId;
                    string name;
                    long ownerUserId;
                    string contentHash;
                    string? fileExt;

                    using (var reader = cmdBase.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Console.WriteLine("Asset not found.");
                            return 1;
                        }

                        assetId = reader.GetInt64(0);
                        assetTypeId = reader.GetInt32(1);
                        name = reader.GetString(2);
                        ownerUserId = reader.GetInt64(3);
                        contentHash = reader.GetString(4);
                        fileExt = reader.IsDBNull(5) ? null : reader.GetString(5);
                    }

                    var idsToDelete = new List<long> { assetId };
                    var filesToDelete = new List<(string Hash, string? Ext)> { (contentHash, fileExt) };

                    if (assetTypeId == 2)
                    {
                        const string linkedSql = "select asset_id, content_hash, file_extension from assets where owner_user_id = @owner and asset_type_id = 1 and name = @name || ' Image' limit 1";
                        using var cmdLinked = new NpgsqlCommand(linkedSql, conn, tx);
                        cmdLinked.Parameters.AddWithValue("owner", ownerUserId);
                        cmdLinked.Parameters.AddWithValue("name", name);
                        using var readerLinked = cmdLinked.ExecuteReader();
                        if (readerLinked.Read())
                        {
                            var linkedId = readerLinked.GetInt64(0);
                            var linkedHash = readerLinked.GetString(1);
                            var linkedExt = readerLinked.IsDBNull(2) ? null : readerLinked.GetString(2);
                            if (!idsToDelete.Contains(linkedId))
                            {
                                idsToDelete.Add(linkedId);
                                filesToDelete.Add((linkedHash, linkedExt));
                            }
                        }
                    }
                    else if (assetTypeId == 1 && name.EndsWith(" Image", StringComparison.Ordinal))
                    {
                        var baseName = name[..^6];
                        const string tshirtSql = "select asset_id, content_hash, file_extension from assets where owner_user_id = @owner and asset_type_id = 2 and name = @name limit 1";
                        using var cmdT = new NpgsqlCommand(tshirtSql, conn, tx);
                        cmdT.Parameters.AddWithValue("owner", ownerUserId);
                        cmdT.Parameters.AddWithValue("name", baseName);
                        using var readerT = cmdT.ExecuteReader();
                        if (readerT.Read())
                        {
                            var linkedId = readerT.GetInt64(0);
                            var linkedHash = readerT.GetString(1);
                            var linkedExt = readerT.IsDBNull(2) ? null : readerT.GetString(2);
                            if (!idsToDelete.Contains(linkedId))
                            {
                                idsToDelete.Add(linkedId);
                                filesToDelete.Add((linkedHash, linkedExt));
                            }
                        }
                    }

                    using (var cmdDel = new NpgsqlCommand("delete from assets where asset_id = any(@ids);", conn, tx))
                    {
                        cmdDel.Parameters.AddWithValue("ids", idsToDelete.ToArray());
                        cmdDel.ExecuteNonQuery();
                    }

                    tx.Commit();

                    if (!string.IsNullOrWhiteSpace(assetFolder) && Directory.Exists(assetFolder))
                    {
                        foreach (var (hash, ext) in filesToDelete)
                        {
                            var fileName = hash + (ext ?? string.Empty);
                            var path = Path.Combine(assetFolder, fileName);
                            if (File.Exists(path))
                            {
                                try
                                {
                                    File.Delete(path);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }

                    Console.WriteLine($"Deleted {idsToDelete.Count} asset(s) and their files where possible.");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    return -1;
                }
            }
            else
            {
                Console.WriteLine("Invalid asset maintenance choice.");
                return 1;
            }
        }
        else if (key == '8')
        {
            try
            {
                Console.Write("Enter user id: ");
                var inputId = Console.ReadLine();
                if (!long.TryParse(inputId, out var uid) || uid <= 0)
                {
                    Console.WriteLine("Invalid user id.");
                    return 1;
                }

                Console.Write("Currency (R)obux or (T)ix? ");
                var curKey = Console.ReadKey(intercept: true).KeyChar;
                Console.WriteLine();
                var currency = curKey == 'T' || curKey == 't' ? "tix" : "robux";

                Console.Write("Amount to add (can be negative): ");
                var amtStr = Console.ReadLine();
                if (!long.TryParse(amtStr, out var delta))
                {
                    Console.WriteLine("Invalid amount.");
                    return 1;
                }

                var column = currency == "tix" ? "tix_balance" : "robux_balance";
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                using var cmd = new NpgsqlCommand($"update users set {column} = {column} + @d where user_id = @id", conn);
                cmd.Parameters.AddWithValue("d", delta);
                cmd.Parameters.AddWithValue("id", uid);
                var affected = cmd.ExecuteNonQuery();
                Console.WriteLine(affected == 1 ? "Balance updated." : "User not found or no change made.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        else if (key == '9')
        {
            Console.WriteLine("Thumbnail maintenance:");
            Console.WriteLine("1) Delete ALL thumbnails from CDN and clear database records");
            Console.WriteLine("2) Delete specific user's thumbnails (avatar/headshot)");
            Console.WriteLine("3) Delete specific asset thumbnails");
            Console.Write("Enter choice (1-3): ");
            var subKey = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            var websiteDir = Path.Combine(repoRoot, "Website");
            string? thumbnailsDirectory = null;
            string? avatar3DDirectory = null;
            
            if (Directory.Exists(websiteDir))
            {
                var cfg = new ConfigurationBuilder()
                    .SetBasePath(websiteDir)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
                thumbnailsDirectory = cfg["Thumbnails:OutputDirectory"];
                avatar3DDirectory = cfg["Thumbnails:Avatar3DDirectory"];
            }

            // Fallback to default CDN location if not configured
            if (string.IsNullOrWhiteSpace(thumbnailsDirectory))
            {
                thumbnailsDirectory = Path.Combine(repoRoot, "CDN", "Assets", "thumbnails");
            }

            if (string.IsNullOrWhiteSpace(avatar3DDirectory))
            {
                avatar3DDirectory = Path.Combine(repoRoot, "CDN", "Assets", "3DAvatar");
            }

            if (subKey == '1')
            {
                Console.WriteLine();
                Console.WriteLine("WARNING: This will DELETE ALL thumbnail files and clear thumbnail-related database records.");
                Console.WriteLine($"Thumbnails directory: {thumbnailsDirectory}");
                Console.WriteLine($"3D Avatar directory: {avatar3DDirectory}");
                Console.Write("Type 'DELETE_ALL_THUMBNAILS' to continue (or anything else to cancel): ");
                var confirm = Console.ReadLine();
                if (!string.Equals(confirm, "DELETE_ALL_THUMBNAILS", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Cancelled.");
                    return 1;
                }

                try
                {
                    using var conn = new NpgsqlConnection(connectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    // Collect thumbnail file hashes before clearing database
                    var thumbnailHashes = new List<string>();
                    
                    // Get user thumbnail hashes from avatar_thumbnail_cache
                    using (var cmd = new NpgsqlCommand("select distinct image_hash from avatar_thumbnail_cache", conn, tx))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var hash = reader.GetString(0);
                            thumbnailHashes.Add(hash);
                        }
                    }

                    // Clear database records
                    using (var cmd = new NpgsqlCommand("delete from avatar_thumbnail_cache", conn, tx))
                        cmd.ExecuteNonQuery();

                    using (var cmd = new NpgsqlCommand("update users set avatar_thumbnail_url = null, headshot_thumbnail_url = null, avatar_render_urls = '{}'::jsonb", conn, tx))
                        cmd.ExecuteNonQuery();

                    using (var cmd = new NpgsqlCommand("update assets set thumbnail_url = null", conn, tx))
                        cmd.ExecuteNonQuery();

                    tx.Commit();

                    // Delete thumbnail files
                    int deletedFiles = 0;
                    if (!string.IsNullOrWhiteSpace(thumbnailsDirectory) && Directory.Exists(thumbnailsDirectory))
                    {
                        foreach (var file in Directory.GetFiles(thumbnailsDirectory, "*", SearchOption.AllDirectories))
                        {
                            try
                            {
                                File.Delete(file);
                                deletedFiles++;
                            }
                            catch
                            {
                                // Ignore file deletion errors
                            }
                        }
                    }

                    // Delete 3D avatar directories
                    int deleted3DFolders = 0;
                    if (!string.IsNullOrWhiteSpace(avatar3DDirectory) && Directory.Exists(avatar3DDirectory))
                    {
                        foreach (var dir in Directory.GetDirectories(avatar3DDirectory))
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                                deleted3DFolders++;
                            }
                            catch
                            {
                                // Ignore directory deletion errors
                            }
                        }
                    }

                    Console.WriteLine($"Deleted {deletedFiles} thumbnail files and {deleted3DFolders} 3D avatar directories.");
                    Console.WriteLine("Cleared all thumbnail-related database records.");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    return -1;
                }
            }
            else if (subKey == '2')
            {
                Console.Write("Enter user id: ");
                var input = Console.ReadLine();
                if (!long.TryParse(input, out var userId) || userId <= 0)
                {
                    Console.WriteLine("Invalid user id.");
                    return 1;
                }

                try
                {
                    using var conn = new NpgsqlConnection(connectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    // Get user's thumbnail hashes from cache
                    var userThumbnailHashes = new List<string>();
                    using (var cmd = new NpgsqlCommand("select image_hash from avatar_thumbnail_cache where config_hash like @pattern", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("pattern", $"%{userId}%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                userThumbnailHashes.Add(reader.GetString(0));
                            }
                        }
                    }

                    // Clear user's thumbnail records
                    using (var cmd = new NpgsqlCommand("delete from avatar_thumbnail_cache where config_hash like @pattern", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("pattern", $"%{userId}%");
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand("update users set avatar_thumbnail_url = null, headshot_thumbnail_url = null, avatar_render_urls = '{}'::jsonb where user_id = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("id", userId);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();

                    // Delete user's thumbnail files
                    int deletedFiles = 0;
                    if (!string.IsNullOrWhiteSpace(thumbnailsDirectory) && Directory.Exists(thumbnailsDirectory))
                    {
                        foreach (var hash in userThumbnailHashes)
                        {
                            var possibleFiles = new[]
                            {
                                Path.Combine(thumbnailsDirectory, hash + ".png"),
                                Path.Combine(thumbnailsDirectory, hash + ".jpg")
                            };

                            foreach (var file in possibleFiles)
                            {
                                if (File.Exists(file))
                                {
                                    try
                                    {
                                        File.Delete(file);
                                        deletedFiles++;
                                    }
                                    catch
                                    {
                                        // Ignore file deletion errors
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine($"Deleted {deletedFiles} thumbnail files for user {userId}.");
                    Console.WriteLine("Cleared user's thumbnail database records.");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    return -1;
                }
            }
            else if (subKey == '3')
            {
                Console.Write("Enter asset id (or 'all' for all assets): ");
                var input = Console.ReadLine();

                try
                {
                    using var conn = new NpgsqlConnection(connectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    List<(long AssetId, string? ThumbnailUrl)> assetsToUpdate = new();

                    if (string.Equals(input, "all", StringComparison.OrdinalIgnoreCase))
                    {
                        // Get all assets with thumbnails
                        using (var cmd = new NpgsqlCommand("select asset_id, thumbnail_url from assets where thumbnail_url is not null", conn, tx))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var assetId = reader.GetInt64(0);
                                var thumbnailUrl = reader.IsDBNull(1) ? null : reader.GetString(1);
                                assetsToUpdate.Add((assetId, thumbnailUrl));
                            }
                        }

                        // Clear all asset thumbnail URLs
                        using (var cmd = new NpgsqlCommand("update assets set thumbnail_url = null", conn, tx))
                            cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        if (!long.TryParse(input, out var assetId) || assetId <= 0)
                        {
                            Console.WriteLine("Invalid asset id.");
                            return 1;
                        }

                        // Get specific asset thumbnail URL
                        using (var cmd = new NpgsqlCommand("select thumbnail_url from assets where asset_id = @id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("id", assetId);
                            var thumbnailUrl = cmd.ExecuteScalar() as string;
                            assetsToUpdate.Add((assetId, thumbnailUrl));
                        }

                        // Clear specific asset thumbnail URL
                        using (var cmd = new NpgsqlCommand("update assets set thumbnail_url = null where asset_id = @id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("id", assetId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();

                    // Delete asset thumbnail files
                    int deletedFiles = 0;
                    if (!string.IsNullOrWhiteSpace(thumbnailsDirectory) && Directory.Exists(thumbnailsDirectory))
                    {
                        foreach (var (_, thumbnailUrl) in assetsToUpdate)
                        {
                            if (!string.IsNullOrWhiteSpace(thumbnailUrl))
                            {
                                // Extract hash from URL (assuming format like /thumbnails/hash.png)
                                var uri = new Uri(thumbnailUrl, UriKind.RelativeOrAbsolute);
                                var fileName = Path.GetFileName(uri.LocalPath);
                                var filePath = Path.Combine(thumbnailsDirectory, fileName);

                                if (File.Exists(filePath))
                                {
                                    try
                                    {
                                        File.Delete(filePath);
                                        deletedFiles++;
                                    }
                                    catch
                                    {
                                        // Ignore file deletion errors
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine($"Deleted {deletedFiles} asset thumbnail files.");
                    Console.WriteLine($"Cleared thumbnail URLs for {assetsToUpdate.Count} asset(s).");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    return -1;
                }
            }
            else
            {
                Console.WriteLine("Invalid thumbnail maintenance choice.");
                return 1;
            }
        }
        else
        {
            Console.WriteLine("Invalid choice.");
            return 1;
        }
    }
}
