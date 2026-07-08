using Npgsql;
using System.Threading.Tasks;

namespace Games;

public static class AliasHandler
{
    public static async Task AppendAliasAsync(string connectionString, long universeId, string aliasJson)
    {
        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand(
            "UPDATE universes SET aliases = aliases || @alias::jsonb WHERE universe_id = @uid", conn);
        cmd.Parameters.AddWithValue("alias", aliasJson);
        cmd.Parameters.AddWithValue("uid", universeId);

        await cmd.ExecuteNonQueryAsync();
    }
}
