using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.ServiceModel;
using System.Threading.Tasks;
using System.ServiceModel.Web;
using System.Collections.Generic;

using Newtonsoft.Json;

using Roblox.Grid;
using Roblox.EventLog;
using Roblox.EventLog.Windows;

namespace Roblox.Thumbnails.Relay
{
    [ServiceContract(Namespace = "http://roblox.com/", ConfigurationName = "ThumbnailsRelay")]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    [XmlSerializerFormat(Style = OperationFormatStyle.Document, Use = OperationFormatUse.Literal)]
    public class ThumbnailsRelay
    {
        private const string _RccServiceRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\ROBLOX Corporation\Roblox";
        private const string _RccServiceRegistryValue = "RccServicePath";

        private static readonly byte[] _EmptyGif = Convert.FromBase64String("R0lGODlhAQABAHAAACH5BAUAAAAALAAAAAABAAEAAAICRAEAOw==");
        private static readonly ILogger _Logger = new Logger(global::Roblox.Thumbnails.Relay.Properties.Settings.Default.LogName, () => global::Roblox.Thumbnails.Relay.Properties.Settings.Default.LogLevel)
        {
            LogToConsole = true,
            LogThreadID = true
        };
        private static readonly PerfStatsMonitor _Monitor = new PerfStatsMonitor();
        private static readonly RccServiceArbiter _RccService = new RccServiceArbiter(global::Roblox.Thumbnails.Relay.Properties.Settings.Default.ArbiterEndpoint);

        private static string _RccVersion;
        private static bool _IsRunning;

        static void Main(string[] args)
        {
            var app = new Roblox.Wcf.ServiceHostApp<ThumbnailsRelay>();
            app.HostOpening += HostApplicationOpening;
            app.HostClosing += HostApplicationClosing;
            app.Process(args);
        }

        #region SOAP Methods

        [OperationContract()]
        public bool IsAlive() => true;

        [OperationContract()]
        public Stream Ping()
        {
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "image/gif";

            return new MemoryStream(_EmptyGif);
        }

        [OperationContract()]
        public ServerStatistics GetStats() => GetServerStatistics();

        [OperationContract()]
        public ThumbnailGenerationResponse RequestThumbnailGenerationEx(ThumbnailType thumbnailType, ImageFormat imageFormat, int width, int height, string assetUrl, string baseUrl, long? mannequinOrUniverseId)
        {
            if (thumbnailType == ThumbnailType.Avatar || thumbnailType == ThumbnailType.AvatarHeadShot)
                throw new Exception("Please use RequestAvatarThumbnailGenerationEx instead for avatar thumbnail types.");

            var luaScript = GetScriptByThumbnailType(thumbnailType);
            var args = GetScriptArgs(thumbnailType, imageFormat, width, height, new object[3] { assetUrl, baseUrl, mannequinOrUniverseId });

            var script = Lua.NewScript(thumbnailType.ToString(), luaScript, args.ToArray());


            var job = new Roblox.Grid.Rcc.Job() { id = Guid.NewGuid().ToString(), category = 2, expirationInSeconds = GetJobTimoutPerThumbnailType(thumbnailType) };

            try
            {
                var response = _RccService.BatchJobEx(job, script);

                if (response == null || response.Length == 0)
                    throw new Exception("Did not receive any LuaValues back from RCC");

                var result = new ThumbnailGenerationResponse();

                if (response.Length == 2)
                {
                    result.Base64EncodedThumbnailData = response[0].value;
                    result.DependencyUrls = (from responseValue in response[1].table
                                             select responseValue.value into depdencyUrl
                                             where !string.IsNullOrEmpty(depdencyUrl)
                                             select depdencyUrl).ToArray();
                }


                return result;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);

                throw;
            }
        }

        // Special action for avatars, essentially the same execution
        [OperationContract()]
        public ThumbnailGenerationResponse RequestAvatarThumbnailGenerationEx(ThumbnailType thumbnailType, ImageFormat imageFormat, int width, int height, string characterAppearaance, string baseUrl, bool? quadratic, int? baseHatZoom, int? maxHatZoom, int? cameraOffsetX, int? cameraOffsetY)
        {
            if (thumbnailType != ThumbnailType.Avatar && thumbnailType != ThumbnailType.AvatarHeadShot)
                throw new Exception("Cannot invoke RequestAvatarThumbnailGenerationEx with non-avatar thumbnail type.");

            var otherArgs = Array.Empty<object>();

            if (thumbnailType == ThumbnailType.Avatar)
                otherArgs = new object[2] { characterAppearaance, baseUrl };
            else
                otherArgs = new object[7] { baseUrl, characterAppearaance, quadratic, baseHatZoom, maxHatZoom, cameraOffsetX, cameraOffsetY };

            var luaScript = GetScriptByThumbnailType(thumbnailType);
            var args = GetScriptArgs(thumbnailType, imageFormat, width, height, otherArgs.ToArray());

            var script = Lua.NewScript(thumbnailType.ToString(), luaScript, args);


            var job = new Roblox.Grid.Rcc.Job() { id = Guid.NewGuid().ToString(), category = 2, expirationInSeconds = GetJobTimoutPerThumbnailType(thumbnailType) };

            try
            {
                var response = _RccService.BatchJobEx(job, script);

                if (response == null || response.Length == 0)
                    throw new Exception("Did not receive any LuaValues back from RCC");

                var result = new ThumbnailGenerationResponse();

                if (response.Length == 2)
                {
                    result.Base64EncodedThumbnailData = response[0].value;
                    result.DependencyUrls = (from responseValue in response[1].table
                                             select responseValue.value into depdencyUrl
                                             where !string.IsNullOrEmpty(depdencyUrl)
                                             select depdencyUrl).ToArray();
                }


                return result;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);

                throw;
            }
        }

        private static int GetJobTimoutPerThumbnailType(ThumbnailType thumbnailType)
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject<IDictionary<string, int>>(global::Roblox.Thumbnails.Relay.Properties.Settings.Default.RccJobLeaseDurationPerThumbnailTypeJsonString);

                if (!parsed.TryGetValue(thumbnailType.ToString(), out var timeout))
                    return global::Roblox.Thumbnails.Relay.Properties.Settings.Default.RccJobLeaseDuration.Seconds;

                return timeout;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);

                return global::Roblox.Thumbnails.Relay.Properties.Settings.Default.RccJobLeaseDuration.Seconds;
            }
        }

        private static IEnumerable<object> GetScriptArgs(ThumbnailType thumbnailType, ImageFormat imageFormat, int width, int height, object[] otherArgs)
        {
            switch (thumbnailType)
            {
                case ThumbnailType.Avatar:
                    yield return otherArgs; // charAppearance, baseUrl
                    yield return imageFormat.ToString();
                    yield return width;
                    yield return height;

                    yield break;
                case ThumbnailType.AvatarHeadShot:
                    var baseUrl = otherArgs[0];
                    var charAppearance = otherArgs[1];

                    yield return baseUrl;
                    yield return charAppearance;
                    yield return imageFormat.ToString();
                    yield return width;
                    yield return height;

                    yield return otherArgs.Skip(2); // quadratic, baseHatZoom, maxHatZoom, cameraOffsetX, cameraOffsetY

                    yield break;
                case ThumbnailType.Face:
                case ThumbnailType.Decal:
                case ThumbnailType.Gear:
                case ThumbnailType.Hat:
                case ThumbnailType.Head:
                case ThumbnailType.MeshPart:
                case ThumbnailType.Mesh:
                case ThumbnailType.Model:
                case ThumbnailType.Pants:
                case ThumbnailType.Place:
                case ThumbnailType.Shirt:

                    yield return otherArgs[0]; // assetUrl

                    yield return imageFormat.ToString();
                    yield return width;
                    yield return height;

                    yield return otherArgs[1]; // baseUrl

                    if (thumbnailType == ThumbnailType.Head ||
                        thumbnailType == ThumbnailType.Pants ||
                        thumbnailType == ThumbnailType.Place ||
                        thumbnailType == ThumbnailType.Shirt)
                        yield return otherArgs[2]; // mannequinId or universeId

                    yield break;
                default:
                    throw new ApplicationException(string.Format("Unknown thumbnail type {0}", thumbnailType));
            }
        }

        private static string GetScriptByThumbnailType(ThumbnailType thumbnailType)
        {
            switch (thumbnailType)
            {
                case ThumbnailType.Avatar:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.AvatarScript;
                case ThumbnailType.AvatarHeadShot:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.AvatarHeadShotScript;
                case ThumbnailType.Face:
                case ThumbnailType.Decal:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.FaceScript;
                case ThumbnailType.Gear:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.GearScript;
                case ThumbnailType.Hat:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.HatScript;
                case ThumbnailType.Head:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.HeadScript;
                case ThumbnailType.MeshPart:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.MeshPartScript;
                case ThumbnailType.Mesh:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.MeshScript;
                case ThumbnailType.Model:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.ModelScript;
                case ThumbnailType.Package:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.PackageScript;
                case ThumbnailType.Pants:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.PantsScript;
                case ThumbnailType.Place:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.PlaceScript;
                case ThumbnailType.Shirt:
                    return global::Roblox.Thumbnails.Relay.Properties.Resources.ShirtScript;
                default:
                    throw new ApplicationException(string.Format("Unknown thumbnail type {0}", thumbnailType));
            }
        }

        #endregion

        #region WCF Methods

        private static void HostApplicationClosing(object sender, EventArgs e)
        {
            _Logger.LifecycleEvent("Stopping...");
            _IsRunning = false;
            _Monitor.Dispose();
        }
        private static void HostApplicationOpening(object sender, EventArgs e)
        {
            _Logger.LifecycleEvent("Starting...");
            _IsRunning = true;
            RunInBackground(_Monitor.Start);
            RunInBackground(CheckRccVersion);
            /*if (string.IsNullOrEmpty(global::Roblox.Thumbnails.Relay.Properties.Settings.Default.ThumbnailsCoordinationServiceUrl))
				return;
			RunInBackground(RequestDispatches);*/
        }

        #endregion

        #region	Version Fetching

        private static void CheckRccVersion()
        {
            while (_IsRunning)
            {
                Thread.Sleep(10000);
                ReadRccVersion();
            }
        }
        private static void ReadRccVersion()
        {
            try
            {
                var regkey = Microsoft.Win32.Registry.GetValue(_RccServiceRegistryKey, _RccServiceRegistryValue, null);
                if (regkey == null) throw new FileNotFoundException("The RccService version is unknown on the current machine.");

                var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Path.Combine(regkey as string, "RCCService.exe"));
                _RccVersion = fileVersionInfo.FileVersion;
            }
            catch (Exception ex)
            {
                _Logger.LifecycleEvent("Error in fetching Rcc Version: {0}", ex);
                _RccVersion = null;
            }
        }

        #endregion

        #region Helpers

        private static void RunInBackground(Action action)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _Logger.Error(ex);
                }
            });
        }

        private static ServerStatistics GetServerStatistics()
        {
            var stats = _Monitor.GetServerStatistics();
            stats.RccVersion = _RccVersion;
            return stats;
        }

        #endregion
    }
}
