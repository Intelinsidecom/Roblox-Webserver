using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Users;

namespace RobloxWebserver.Controllers
{

    [ApiController]
    [Route("comments")]
    public class CommentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CommentsController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private sealed class StoredComment
        {
            [JsonPropertyName("Id")]
            public long Id { get; set; }

            [JsonPropertyName("AuthorId")]
            public long AuthorId { get; set; }

            [JsonPropertyName("AuthorName")]
            public string AuthorName { get; set; } = string.Empty;

            [JsonPropertyName("Text")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("PostedAtUtc")]
            public DateTime PostedAtUtc { get; set; }
        }

        // GET /comments/get-json?assetId=...&startindex=0&thumbnailWidth=100&thumbnailHeight=100&thumbnailFormat=PNG&cachebuster=...
        [HttpGet("get-json")]
        public async Task<IActionResult> GetJson(
            [FromQuery] long assetId,
            [FromQuery(Name = "startindex")] int startIndex = 0,
            [FromQuery] int thumbnailWidth = 100,
            [FromQuery] int thumbnailHeight = 100,
            [FromQuery] string? thumbnailFormat = "PNG",
            [FromQuery] int cachebuster = 0,
            CancellationToken cancellationToken = default)
        {
            const int pageSize = 10;

            if (assetId <= 0)
            {
                var empty = new
                {
                    Comments = Array.Empty<object>(),
                    MaxRows = pageSize,
                    IsUserModerator = false
                };

                var emptyJson = JsonSerializer.Serialize(empty, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });

                return Content(emptyJson, "application/json");
            }

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                var error = new
                {
                    Comments = Array.Empty<object>(),
                    MaxRows = pageSize,
                    IsUserModerator = false
                };

                var errorJson = JsonSerializer.Serialize(error, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });

                return Content(errorJson, "application/json");
            }

            List<StoredComment> allComments = new List<StoredComment>();

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string sql = "select comments from assets where asset_id = @id";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", assetId);

                var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (obj != null && obj != DBNull.Value)
                {
                    var json = obj.ToString();
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            var parsed = JsonSerializer.Deserialize<List<StoredComment>>(json);
                            if (parsed != null)
                            {
                                allComments = parsed;
                            }
                        }
                        catch
                        {
                            allComments = new List<StoredComment>();
                        }
                    }
                }
            }
            catch
            {
                allComments = new List<StoredComment>();
            }

            var ordered = allComments
                .OrderByDescending(c => c.PostedAtUtc)
                .ThenByDescending(c => c.Id)
                .ToList();

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            var page = ordered
                .Skip(startIndex)
                .Take(pageSize)
                .Select(c => new
                {
                    Id = c.Id,
                    PostedDate = c.PostedAtUtc.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
                    Text = c.Text,
                    AuthorName = c.AuthorName,
                    AuthorId = c.AuthorId,
                    AuthorThumbnail = new
                    {
                        Url = "/images/RobloxLogo.png"
                    }
                })
                .ToList();

            var response = new
            {
                Comments = page,
                MaxRows = pageSize,
                IsUserModerator = false
            };

            var jsonOut = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });

            return Content(jsonOut, "application/json");
        }

        // POST /comments/post (used by Website/wwwroot/JS/Comments.js)
        [Authorize]
        [HttpPost("post")]
        public async Task<IActionResult> PostComment(
            [FromForm] long assetId,
            [FromForm] string text,
            CancellationToken cancellationToken = default)
        {
            if (assetId <= 0 || string.IsNullOrWhiteSpace(text))
            {
                return new JsonResult(new { error = "Invalid request" });
            }

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                return new JsonResult(new { error = "Database connection string is not configured." });
            }

            var userIdStr = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId) || userId <= 0)
            {
                return new JsonResult(new { error = "Authentication required" });
            }

            string authorName = string.Empty;
            try
            {
                authorName = await UserQueries.GetUserNameByIdAsync(connStr, userId).ConfigureAwait(false) ?? string.Empty;
            }
            catch
            {
                authorName = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(authorName))
            {
                authorName = $"User_{userId}";
            }

            List<StoredComment> comments = new List<StoredComment>();

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string selectSql = "select comments from assets where asset_id = @id for update";
                await using (var tx = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
                {
                    await using (var selectCmd = new NpgsqlCommand(selectSql, conn, tx))
                    {
                        selectCmd.Parameters.AddWithValue("id", assetId);
                        var obj = await selectCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                        if (obj != null && obj != DBNull.Value)
                        {
                            var existingJson = obj.ToString();
                            if (!string.IsNullOrWhiteSpace(existingJson))
                            {
                                try
                                {
                                    var parsed = JsonSerializer.Deserialize<List<StoredComment>>(existingJson);
                                    if (parsed != null)
                                    {
                                        comments = parsed;
                                    }
                                }
                                catch
                                {
                                    comments = new List<StoredComment>();
                                }
                            }
                        }
                    }

                    long nextId = comments.Count == 0 ? 1 : comments.Max(c => c.Id) + 1;

                    var newComment = new StoredComment
                    {
                        Id = nextId,
                        AuthorId = userId,
                        AuthorName = authorName,
                        Text = text,
                        PostedAtUtc = DateTime.UtcNow
                    };

                    comments.Add(newComment);

                    var updatedJson = JsonSerializer.Serialize(comments, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null
                    });

                    const string updateSql = "update assets set comments = @comments::jsonb where asset_id = @id";
                    await using (var updateCmd = new NpgsqlCommand(updateSql, conn, tx))
                    {
                        updateCmd.Parameters.AddWithValue("id", assetId);
                        updateCmd.Parameters.AddWithValue("comments", updatedJson ?? "[]");
                        await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    }

                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                    var response = new
                    {
                        Id = newComment.Id,
                        PostedDate = newComment.PostedAtUtc.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
                        Text = newComment.Text,
                        AuthorName = newComment.AuthorName,
                        AuthorId = newComment.AuthorId
                    };

                    return new JsonResult(response);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        // POST /comments/delete (used by Website/wwwroot/JS/Comments.js)
        [Authorize]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteComment(
            [FromForm] long commentId,
            [FromForm] long? assetId,
            CancellationToken cancellationToken = default)
        {
            if (commentId <= 0)
            {
                return new JsonResult(new { error = "Invalid commentId" });
            }

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                return new JsonResult(new { error = "Database connection string is not configured." });
            }

            var userIdStr = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId) || userId <= 0)
            {
                return new JsonResult(new { error = "Authentication required" });
            }

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string selectSql = "select asset_id, comments from assets where (@aid is null or asset_id = @aid)";

                long targetAssetId = 0;
                List<StoredComment> comments = new List<StoredComment>();

                await using (var tx = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
                {
                    await using (var selectCmd = new NpgsqlCommand(selectSql, conn, tx))
                    {
                        selectCmd.Parameters.AddWithValue("aid", (object?)assetId ?? DBNull.Value);

                        await using var reader = await selectCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            var aid = reader.GetInt64(0);
                            var obj = reader.GetValue(1);
                            if (obj == null || obj == DBNull.Value)
                            {
                                continue;
                            }

                            var json = obj.ToString();
                            if (string.IsNullOrWhiteSpace(json))
                            {
                                continue;
                            }

                            List<StoredComment>? parsed;
                            try
                            {
                                parsed = JsonSerializer.Deserialize<List<StoredComment>>(json);
                            }
                            catch
                            {
                                parsed = null;
                            }

                            if (parsed == null)
                            {
                                continue;
                            }

                            if (parsed.Any(c => c.Id == commentId && (c.AuthorId == userId)))
                            {
                                targetAssetId = aid;
                                comments = parsed;
                                break;
                            }
                        }
                    }

                    if (targetAssetId == 0 || comments.Count == 0)
                    {
                        await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                        return new JsonResult(new { error = "Comment not found" });
                    }

                    comments = comments
                        .Where(c => !(c.Id == commentId && c.AuthorId == userId))
                        .ToList();

                    var updatedJson = JsonSerializer.Serialize(comments, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null
                    });

                    const string updateSql = "update assets set comments = @comments::jsonb where asset_id = @id";
                    await using (var updateCmd = new NpgsqlCommand(updateSql, conn, tx))
                    {
                        updateCmd.Parameters.AddWithValue("id", targetAssetId);
                        updateCmd.Parameters.AddWithValue("comments", updatedJson ?? "[]");
                        await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    }

                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                    return new JsonResult(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }
    }
}
