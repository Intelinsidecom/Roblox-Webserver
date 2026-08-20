using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Common;
using Games;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RobloxWebserver.Controllers.Client
{
    [ApiController]
    [Route("persistence")]
    public class DatastoreController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DatastoreController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => DatabaseUtilities.GetConnectionString(_configuration);

        private long? PlaceId
        {
            get
            {
                var value = Request.Query["placeId"].FirstOrDefault();
                return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var placeId) ? placeId : (long?)null;
            }
        }

        // POST /persistence/getV2?placeId=&type=&scope=
        // Body (form-urlencoded): qkeys[0].scope=...&qkeys[0].target=...&qkeys[0].key=...
        [HttpPost("getV2")]
        public async Task<IActionResult> GetV2(CancellationToken cancellationToken)
        {
            try
            {
                var placeId = PlaceId;
                if (!placeId.HasValue)
                    return JsonResponse(new JObject { ["error"] = "Invalid place ID" });

                var queries = Datastore.ParseQueries(Request.Form.Keys, key => Request.Form[key].ToString());
                if (queries.Count == 0)
                    return JsonResponse(new JObject { ["data"] = new JArray() });

                var result = await Datastore.GetAsync(ConnectionString, placeId.Value, queries, cancellationToken);
                return JsonResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        // POST /persistence/set?placeId=&key=&type=&scope=&target=&valueLength=[&expectedValueLength=]
        // Body (form-urlencoded): value=... [&expectedValue=...]
        [HttpPost("set")]
        public async Task<IActionResult> Set(CancellationToken cancellationToken)
        {
            try
            {
                var placeId = PlaceId;
                if (!placeId.HasValue)
                    return JsonResponse(new JObject { ["error"] = "Invalid place ID" });

                var scope = Request.Query["scope"].FirstOrDefault() ?? "";
                var target = Request.Query["target"].FirstOrDefault() ?? "";
                var key = Request.Query["key"].FirstOrDefault() ?? "";
                var type = Request.Query["type"].FirstOrDefault() ?? "standard";
                var isOrdered = string.Equals(type, "sorted", StringComparison.OrdinalIgnoreCase);

                var value = Request.Form["value"].FirstOrDefault();
                if (value == null)
                    return JsonResponse(new JObject { ["error"] = "Missing value" });

                string? expectedValue = null;
                if (Request.Form.ContainsKey("expectedValue"))
                    expectedValue = Request.Form["expectedValue"].FirstOrDefault() ?? "";

                var result = await Datastore.SetAsync(ConnectionString, placeId.Value, scope, target, key, value, expectedValue, isOrdered, cancellationToken);
                return JsonResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        // POST /persistence/increment?placeId=&key=&type=&scope=&target=&value=
        [HttpPost("increment")]
        public async Task<IActionResult> Increment(CancellationToken cancellationToken)
        {
            try
            {
                var placeId = PlaceId;
                if (!placeId.HasValue)
                    return JsonResponse(new JObject { ["error"] = "Invalid place ID" });

                var scope = Request.Query["scope"].FirstOrDefault() ?? "";
                var target = Request.Query["target"].FirstOrDefault() ?? "";
                var key = Request.Query["key"].FirstOrDefault() ?? "";

                if (!long.TryParse(Request.Query["value"].FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var delta))
                    return JsonResponse(new JObject { ["error"] = "Invalid delta" });

                var result = await Datastore.IncrementAsync(ConnectionString, placeId.Value, scope, target, key, delta, cancellationToken);
                return JsonResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        // POST /persistence/getSortedValues?placeId=&type=sorted&scope=&key=&pageSize=&ascending=[&inclusiveMinValue=&inclusiveMaxValue=&exclusiveStartKey=]
        [HttpPost("getSortedValues")]
        public async Task<IActionResult> GetSortedValues(CancellationToken cancellationToken)
        {
            try
            {
                var placeId = PlaceId;
                if (!placeId.HasValue)
                    return JsonResponse(new JObject { ["error"] = "Invalid place ID" });

                var scope = Request.Query["scope"].FirstOrDefault() ?? "";
                var key = Request.Query["key"].FirstOrDefault() ?? "";

                var pageSize = int.TryParse(Request.Query["pageSize"].FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ps) ? ps : 100;
                var ascending = !string.Equals(Request.Query["ascending"].FirstOrDefault(), "False", StringComparison.OrdinalIgnoreCase);
                var inclusiveMin = ParseNullableDouble(Request.Query["inclusiveMinValue"].FirstOrDefault());
                var inclusiveMax = ParseNullableDouble(Request.Query["inclusiveMaxValue"].FirstOrDefault());
                var exclusiveStartKey = Request.Query["exclusiveStartKey"].FirstOrDefault();

                var result = await Datastore.GetSortedValuesAsync(ConnectionString, placeId.Value, scope, key, pageSize, ascending, inclusiveMin, inclusiveMax, exclusiveStartKey, cancellationToken);
                return JsonResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message);
            }
        }

        private static double? ParseNullableDouble(string? value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : (double?)null;
        }

        private static ContentResult JsonResponse(JObject obj)
        {
            return new ContentResult
            {
                StatusCode = 200,
                ContentType = "application/json",
                Content = obj.ToString(Formatting.None)
            };
        }

        private static ContentResult ErrorResponse(string message)
        {
            return new ContentResult
            {
                StatusCode = 500,
                ContentType = "application/json",
                Content = new JObject { ["error"] = message }.ToString(Formatting.None)
            };
        }
    }
}
