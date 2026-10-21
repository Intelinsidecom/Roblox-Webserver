using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public sealed class XboxCatalogProduct
    {
        public long AssetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int AssetTypeId { get; set; }
        public long PriceInRobux { get; set; }
        public bool IsPublicDomain { get; set; }
        public bool IsForSale { get; set; }
        public long CreatorId { get; set; }
        public string CreatorName { get; set; } = string.Empty;
    }

    public static class XboxCatalogQueries
    {
        public const int PackageAssetTypeId = 32;

        public static async Task<List<XboxCatalogProduct>> GetCatalogPackagesAsync(
            string connectionString,
            int startIndex,
            int count,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            if (startIndex < 0) startIndex = 0;
            if (count < 1) count = 20;
            if (count > 50) count = 50;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"
select a.asset_id, a.name, a.description, a.asset_type_id, a.price, a.on_sale,
       a.owner_user_id, coalesce(u.user_name, '')
from assets a
left join users u on u.user_id = a.owner_user_id
where a.asset_type_id = @typeId
order by a.asset_id desc
limit @count offset @offset";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("typeId", PackageAssetTypeId);
            cmd.Parameters.AddWithValue("count", count);
            cmd.Parameters.AddWithValue("offset", startIndex);

            var products = new List<XboxCatalogProduct>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var price = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);
                products.Add(new XboxCatalogProduct
                {
                    AssetId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    AssetTypeId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    PriceInRobux = price,
                    IsPublicDomain = price <= 0,
                    IsForSale = !reader.IsDBNull(5) && reader.GetBoolean(5),
                    CreatorId = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                    CreatorName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                });
            }
            return products;
        }
    }
}
