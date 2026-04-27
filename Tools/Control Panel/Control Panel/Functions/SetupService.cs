using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Control_Panel;
using Control_Panel.Properties;

namespace ControlPanel.Functions
{
    /// <summary>
    /// Refactored SetupService that works locally with files and database instead of HTTP requests
    /// </summary>
    public class SetupService
    {
        private readonly string _clientsInputFolder;
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<SetupService> _logger;
        private readonly System.Windows.Threading.Dispatcher _uiDispatcher;
        
        public SetupService(string clientsInputFolder, NpgsqlConnection dbConnection, ILogger<SetupService> logger)
        {
            _clientsInputFolder = clientsInputFolder ?? throw new ArgumentNullException(nameof(clientsInputFolder));
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _uiDispatcher = System.Windows.Application.Current?.Dispatcher;
            _logger = logger ?? new ConsoleWindowLogger(_uiDispatcher);
            
            if (!Directory.Exists(_clientsInputFolder))
            {
                Directory.CreateDirectory(_clientsInputFolder);
            }
        }
        
        private void LogInfo(string message)
        {
            SafeLogToConsoleWindow(() => ConsoleWindow.Instance?.WriteLine(message), $"INFO: {message}");
        }
        
        private void LogError(string message)
        {
            SafeLogToConsoleWindow(() => ConsoleWindow.Instance?.WriteError(message), $"ERROR: {message}");
        }
        
        private void LogError(string message, Exception ex)
        {
            var fullMessage = $"{message}: {ex.Message}";
            SafeLogToConsoleWindow(() => ConsoleWindow.Instance?.WriteError(fullMessage), $"ERROR: {fullMessage}");
        }
        
        private void LogWarning(string message)
        {
            SafeLogToConsoleWindow(() => ConsoleWindow.Instance?.WriteWarning(message), $"WARNING: {message}");
        }
        
        private void SafeLogToConsoleWindow(Action logAction, string fallbackMessage)
        {
            try
            {
                if (_uiDispatcher?.CheckAccess() == true)
                {
                    logAction();
                }
                else
                {
                    _uiDispatcher?.BeginInvoke((Action)(() =>
                    {
                        try
                        {
                            logAction();
                        }
                        catch
                        {
                            System.Console.WriteLine(fallbackMessage);
                        }
                    }));
                }
            }
            catch
            {
                System.Console.WriteLine(fallbackMessage);
            }
        }
        
        /// <summary>
        /// Gets client version from database setup table
        /// </summary>
        public async Task<string> GetClientVersionAsync(string clientType)
        {
            try
            {
                await _dbConnection.OpenAsync();
                
                using var command = _dbConnection.CreateCommand();
                command.CommandText = "SELECT get_client_version(@clientType)";
                command.Parameters.Add(new NpgsqlParameter("@clientType", clientType));
                
                var result = await command.ExecuteScalarAsync();
                var version = result?.ToString() ?? "";
                return version;
            }
            catch (Exception ex)
            {
                LogError($"Failed to get {clientType} version from database", ex);
                throw new Exception($"Error getting client version: {ex.Message}", ex);
            }
            finally
            {
                await _dbConnection.CloseAsync();
            }
        }
        
        /// <summary>
        /// Gets CDN configuration from local file
        /// </summary>
        public async Task<string> GetCdnConfigAsync()
        {
            try
            {
                var cdnConfigPath = Path.Combine(_clientsInputFolder, "cdn.txt");
                
                if (!File.Exists(cdnConfigPath))
                {
                    _logger.LogWarning($"CDN config file not found: {cdnConfigPath}");
                    return "";
                }
                
                var content = await File.ReadAllTextAsync(cdnConfigPath);
                return content;
            }
            catch (Exception ex)
            {
                LogError("Failed to read CDN config file");
                throw new Exception($"Error reading CDN config: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets installer CDNs from local configuration file
        /// </summary>
        public async Task<Dictionary<string, int>> GetInstallerCdnsAsync()
        {
            try
            {
                var installerConfigPath = Path.Combine(_clientsInputFolder, "installer-cdns.json");
                
                if (!File.Exists(installerConfigPath))
                {
                    _logger.LogWarning($"Installer CDNs config file not found: {installerConfigPath}");
                    return new Dictionary<string, int>();
                }
                
                var json = await File.ReadAllTextAsync(installerConfigPath);
                var cdns = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                
                return cdns ?? new Dictionary<string, int>();
            }
            catch (Exception ex)
            {
                LogError("Failed to read installer CDNs config file");
                throw new Exception($"Error reading installer CDNs: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets bootstrapper settings from local file
        /// </summary>
        public async Task<BootstrapperSettings> GetBootstrapperSettingsAsync()
        {
            try
            {
                var settingsPath = Path.Combine(_clientsInputFolder, "bootstrapper-settings.json");
                
                if (!File.Exists(settingsPath))
                {
                    return new BootstrapperSettings();
                }
                
                var json = await File.ReadAllTextAsync(settingsPath);
                var settings = System.Text.Json.JsonSerializer.Deserialize<BootstrapperSettings>(json);
                
                return settings ?? new BootstrapperSettings();
            }
            catch (Exception ex)
            {
                LogError("Failed to read bootstrapper settings file");
                throw new Exception($"Error reading bootstrapper settings: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets version GUID from local file
        /// </summary>
        public async Task<string> GetVersionGuidAsync(string product, int guid = 0)
        {
            try
            {
                var versionFilePath = Path.Combine(_clientsInputFolder, $"{product}-{guid}.txt");
                
                if (!File.Exists(versionFilePath))
                {
                    _logger.LogWarning($"Version file not found: {versionFilePath}");
                    return "";
                }
                
                var content = await File.ReadAllTextAsync(versionFilePath);
                return content.Trim();
            }
            catch (Exception ex)
            {
                LogError($"Failed to read version file for {product}");
                throw new Exception($"Error getting version GUID: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets bootstrapper version from local file
        /// </summary>
        public async Task<string> GetBootstrapperVersionAsync(string version)
        {
            try
            {
                var versionFilePath = Path.Combine(_clientsInputFolder, $"{version}-version.txt");
                
                if (!File.Exists(versionFilePath))
                {
                    _logger.LogWarning($"Bootstrapper version file not found: {versionFilePath}");
                    return "";
                }
                
                var content = await File.ReadAllTextAsync(versionFilePath);
                return content.Trim();
            }
            catch (Exception ex)
            {
                LogError("Failed to read bootstrapper version file", ex);
                throw new Exception($"Error getting bootstrapper version: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets manifest from local file
        /// </summary>
        public async Task<string> GetManifestAsync(string version)
        {
            try
            {
                var manifestPath = Path.Combine(_clientsInputFolder, $"{version}-rbxManifest.txt");
                
                if (!File.Exists(manifestPath))
                {
                    _logger.LogWarning($"Manifest file not found: {manifestPath}");
                    return "";
                }
                
                var content = await File.ReadAllTextAsync(manifestPath);
                return content;
            }
            catch (Exception ex)
            {
                LogError("Failed to read manifest file", ex);
                throw new Exception($"Error getting manifest: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets deploy history from local file
        /// </summary>
        public async Task<string> GetDeployHistoryAsync()
        {
            try
            {
                var historyPath = Path.Combine(_clientsInputFolder, "DeployHistory.txt");
                
                if (!File.Exists(historyPath))
                {
                    _logger.LogWarning($"Deploy history file not found: {historyPath}");
                    return "";
                }
                
                var content = await File.ReadAllTextAsync(historyPath);
                return content;
            }
            catch (Exception ex)
            {
                LogError("Failed to read deploy history file", ex);
                throw new Exception($"Error getting deploy history: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Reverts client to specific version by deleting version files if not current
        /// </summary>
        public async Task<RevertResult> RevertClientAsync(string clientType, string targetHash)
        {
            try
            {

                var dbClientType = MapClientType(clientType);
                if (string.IsNullOrEmpty(dbClientType))
                {
                    return new RevertResult
                    {
                        Success = false,
                        Error = $"Invalid client type: {clientType}"
                    };
                }
                
                await _dbConnection.OpenAsync();
                
                using var getCurrentCommand = _dbConnection.CreateCommand();
                getCurrentCommand.CommandText = "SELECT get_client_version(@clientType)";
                getCurrentCommand.Parameters.Add(new NpgsqlParameter("@clientType", dbClientType));
                
                var currentVersionResult = await getCurrentCommand.ExecuteScalarAsync();
                var currentVersion = currentVersionResult?.ToString();
                
                if (string.IsNullOrEmpty(currentVersion))
                {
                    return new RevertResult
                    {
                        Success = false,
                        Error = $"Could not determine current version for {clientType}"
                    };
                }
                
                if (currentVersion == targetHash)
                {
                    return new RevertResult
                    {
                        Success = false,
                        Error = $"Cannot revert to {targetHash} - it's already the current version"
                    };
                }
                
                var setupServiceLocation = GetSetupServiceLocation();
                var wwwrootPath = Path.Combine(setupServiceLocation, "wwwroot");
                
                if (!Directory.Exists(wwwrootPath))
                {
                    return new RevertResult
                    {
                        Success = false,
                        Error = "wwwroot directory not found"
                    };
                }
                
                var filesToDelete = Directory.GetFiles(wwwrootPath, $"version-{targetHash}*");
                var deletedFiles = new List<string>();
                
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        File.Delete(file);
                        deletedFiles.Add(Path.GetFileName(file));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to delete file {file}: {ex.Message}");
                    }
                }
                
                if (deletedFiles.Count == 0)
                {
                    return new RevertResult
                    {
                        Success = false,
                        Error = $"No files found for version {targetHash}"
                    };
                }
                
                await WriteToDeploymentHistoryAsync($"Revert {clientType} version-{targetHash} at {DateTime.Now:dd/MM/yyyy h:mm:ss tt}");
                
                return new RevertResult
                {
                    Success = true,
                    RevertedHash = targetHash,
                    Message = $"Successfully reverted {clientType} to {targetHash}, deleted {deletedFiles.Count} files",
                    RevertedFiles = deletedFiles
                };
            }
            catch (Exception ex)
            {
                LogError("Error reverting {clientType} to {targetHash}", ex);
                return new RevertResult
                {
                    Success = false,
                    Error = $"Revert failed: {ex.Message}"
                };
            }
            finally
            {
                if (_dbConnection.State == System.Data.ConnectionState.Open)
                {
                    await _dbConnection.CloseAsync();
                }
            }
        }
        
        /// <summary>
        /// Writes entry to local deployment history file
        /// </summary>
        private async Task WriteToDeploymentHistoryAsync(string entry)
        {
            try
            {
                var setupServiceLocation = GetSetupServiceLocation();
                var wwwrootPath = Path.Combine(setupServiceLocation, "wwwroot");
                Directory.CreateDirectory(wwwrootPath);
                
                var historyPath = Path.Combine(wwwrootPath, "DeployHistory.txt");
                
                var currentHistory = "";
                if (File.Exists(historyPath))
                {
                    currentHistory = await File.ReadAllTextAsync(historyPath);
                }
                
                var updatedHistory = string.IsNullOrEmpty(currentHistory) 
                    ? entry + "...Done!"
                    : $"{currentHistory}\n{entry}...Done!";
                
                await File.WriteAllTextAsync(historyPath, updatedHistory);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write deployment history");
            }
        }
        
        /// <summary>
        /// Uploads and packages Player client files with MD5 hash version
        /// </summary>
        public async Task<UploadResult> UploadPlayerClientAsync(string folderPath, string bootstrapperPath = null, string bootstrapperVersion = null)
        {
            try
            {
                // Validate folder exists
                if (!Directory.Exists(folderPath))
                {
                    return new UploadResult
                    {
                        Success = false,
                        Error = $"Folder not found: {folderPath}"
                    };
                }
                
                var exeName = "RobloxPlayerBeta.exe";
                var exePath = Path.Combine(folderPath, exeName);
                
                if (!File.Exists(exePath))
                {
                    throw new FileNotFoundException($"Required executable not found: {exeName} in {folderPath}");
                }
                
                if (string.IsNullOrEmpty(bootstrapperVersion))
                {
                    return new UploadResult
                    {
                        Success = false,
                        Error = "Bootstrapper version is required."
                    };
                }
                
                var version = bootstrapperVersion;
                
                var setupServiceLocation = GetSetupServiceLocation();
                var wwwrootPath = Path.Combine(setupServiceLocation, "wwwroot");
                Directory.CreateDirectory(wwwrootPath);
                
                var createdZipFiles = new List<string>();
                
                var packageFiles = new List<(string sourcePath, string entryName)>
                {
                    (exePath, exeName),
                    (Path.Combine(folderPath, "AppSettings.xml"), "AppSettings.xml"),
                    (Path.Combine(folderPath, "ReflectionMetadata.xml"), "ReflectionMetadata.xml"),
                };
                
                var zipFileName = $"version-{version}-RobloxApp.zip";
                var zipFilePath = Path.Combine(wwwrootPath, zipFileName);
                var packagedFiles = await CreateZipPackage(zipFilePath, packageFiles);
                
                if (string.IsNullOrEmpty(bootstrapperPath))
                {
                    return new UploadResult
                    {
                        Success = false,
                        Error = "Bootstrapper path is required."
                    };
                }
                
                if (!File.Exists(bootstrapperPath))
                {
                    return new UploadResult
                    {
                        Success = false,
                        Error = $"Bootstrapper file not found: {bootstrapperPath}"
                    };
                }
                
                var bootstrapperFileName = $"version-{version}-Roblox.exe";
                var bootstrapperDestPath = Path.Combine(wwwrootPath, bootstrapperFileName);
                
                File.Copy(bootstrapperPath, bootstrapperDestPath, true);
                packagedFiles.Add(bootstrapperFileName);                
                var launcherPath = Path.Combine(wwwrootPath, "RobloxPlayerLauncher.exe");
                File.Copy(bootstrapperPath, launcherPath, overwrite: true);
                
                createdZipFiles.Add("RobloxApp.zip");
                
                var shadersPath = Path.Combine(folderPath, "shaders");
                if (Directory.Exists(shadersPath))
                {
                    var shaderFiles = new List<(string sourcePath, string entryName)>();
                    var shaderFilePaths = Directory.GetFiles(shadersPath, "*", SearchOption.AllDirectories);
                    
                    foreach (var shaderFile in shaderFilePaths)
                    {
                        var relativePath = Path.GetRelativePath(shadersPath, shaderFile);
                        shaderFiles.Add((shaderFile, relativePath));
                    }
                    
                    if (shaderFiles.Count > 0)
                    {
                        var shadersZipFileName = $"version-{version}-shaders.zip";
                        var shadersZipFilePath = Path.Combine(wwwrootPath, shadersZipFileName);
                        var packagedShaderFiles = await CreateZipPackage(shadersZipFilePath, shaderFiles);
                        packagedFiles.AddRange(packagedShaderFiles.Select(f => $"shaders/{f}"));
                    }
                }
                
                var proxyFiles = new List<(string sourcePath, string entryName)>
                {
                    (Path.Combine(folderPath, "RobloxProxy.dll"), "RobloxProxy.dll"),
                    (Path.Combine(folderPath, "RobloxProxy64.dll"), "RobloxProxy64.dll")
                };
                
                var existingProxyFiles = proxyFiles.Where(f => File.Exists(f.sourcePath)).ToList();
                if (existingProxyFiles.Count > 0)
                {
                    var proxyZipFileName = $"version-{version}-RobloxProxy.zip";
                    var proxyZipFilePath = Path.Combine(wwwrootPath, proxyZipFileName);
                    var packagedProxyFiles = await CreateZipPackage(proxyZipFilePath, existingProxyFiles);
                    packagedFiles.AddRange(packagedProxyFiles.Select(f => $"proxy/{f}"));
                }
                
                var npProxyFiles = new List<(string sourcePath, string entryName)>
                {
                    (Path.Combine(folderPath, "NPRobloxProxy.dll"), "NPRobloxProxy.dll"),
                    (Path.Combine(folderPath, "NPRobloxProxy64.dll"), "NPRobloxProxy64.dll")
                };
                
                var existingNpProxyFiles = npProxyFiles.Where(f => File.Exists(f.sourcePath)).ToList();
                if (existingNpProxyFiles.Count > 0)
                {
                    var npProxyZipFileName = $"version-{version}-NPRobloxProxy.zip";
                    var npProxyZipFilePath = Path.Combine(wwwrootPath, npProxyZipFileName);
                    var packagedNpProxyFiles = await CreateZipPackage(npProxyZipFilePath, existingNpProxyFiles);
                    packagedFiles.AddRange(packagedNpProxyFiles.Select(f => $"proxy/{f}"));
                }
                
                var platformContentPath = Path.Combine(folderPath, "PlatformContent");
                if (Directory.Exists(platformContentPath))
                {
                    var pcContentPath = Path.Combine(platformContentPath, "pc");
                    
                    var pcTexturesPath = Path.Combine(pcContentPath, "textures");
                    if (Directory.Exists(pcTexturesPath))
                    {
                        var pcTextureFiles = new List<(string sourcePath, string entryName)>();
                        var pcTextureFilePaths = Directory.GetFiles(pcTexturesPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var pcTextureFile in pcTextureFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(pcTexturesPath, pcTextureFile);
                            pcTextureFiles.Add((pcTextureFile, relativePath));
                        }
                        
                        if (pcTextureFiles.Count > 0)
                        {
                            var pcTexturesZipFileName = $"version-{version}-content-textures3.zip";
                            var pcTexturesZipFilePath = Path.Combine(wwwrootPath, pcTexturesZipFileName);
                            var packagedPcTextureFiles = await CreateZipPackage(pcTexturesZipFilePath, pcTextureFiles);
                            packagedFiles.AddRange(packagedPcTextureFiles.Select(f => $"content/{f}"));
                        }
                    }
                    
                    var pcTerrainPath = Path.Combine(pcContentPath, "terrain");
                    if (Directory.Exists(pcTerrainPath))
                    {
                        var pcTerrainFiles = new List<(string sourcePath, string entryName)>();
                        var pcTerrainFilePaths = Directory.GetFiles(pcTerrainPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var pcTerrainFile in pcTerrainFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(pcTerrainPath, pcTerrainFile);
                            pcTerrainFiles.Add((pcTerrainFile, relativePath));
                        }
                        
                        if (pcTerrainFiles.Count > 0)
                        {
                            var pcTerrainZipFileName = $"version-{version}-content-terrain.zip";
                            var pcTerrainZipFilePath = Path.Combine(wwwrootPath, pcTerrainZipFileName);
                            var packagedPcTerrainFiles = await CreateZipPackage(pcTerrainZipFilePath, pcTerrainFiles);
                            packagedFiles.AddRange(packagedPcTerrainFiles.Select(f => $"content/{f}"));
                        }
                    }
                }
                
                var requiredDlls = new List<(string sourcePath, string entryName)>
                {
                    (Path.Combine(folderPath, "SDL2.dll"), "SDL2.dll"),
                    (Path.Combine(folderPath, "VMProtectSDK32.dll"), "VMProtectSDK32.dll"),
                    (Path.Combine(folderPath, "fmod.dll"), "fmod.dll"),
                    (Path.Combine(folderPath, "d3dcompiler_47.dll"), "d3dcompiler_47.dll"),
                    (Path.Combine(folderPath, "boost.dll"), "boost.dll"),
                    (Path.Combine(folderPath, "openvr_api.dll"), "openvr_api.dll")
                };
                
                var existingDlls = requiredDlls.Where(f => File.Exists(f.sourcePath)).ToList();
                if (existingDlls.Count > 0)
                {
                    var dllZipFileName = $"version-{version}-Libraries.zip";
                    var dllZipFilePath = Path.Combine(wwwrootPath, dllZipFileName);
                    var packagedDllFiles = await CreateZipPackage(dllZipFilePath, existingDlls);
                    packagedFiles.AddRange(packagedDllFiles.Select(f => $"libs/{f}"));
                }
                
                var contentPath = Path.Combine(folderPath, "content");
                if (Directory.Exists(contentPath))
                {
                    var fontsPath = Path.Combine(contentPath, "fonts");
                    if (Directory.Exists(fontsPath))
                    {
                        var fontFiles = new List<(string sourcePath, string entryName)>();
                        var fontFilePaths = Directory.GetFiles(fontsPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var fontFile in fontFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(fontsPath, fontFile);
                            fontFiles.Add((fontFile, relativePath));
                        }
                        
                        if (fontFiles.Count > 0)
                        {
                            var fontsZipFileName = $"version-{version}-content-fonts.zip";
                            var fontsZipFilePath = Path.Combine(wwwrootPath, fontsZipFileName);
                            var packagedFontFiles = await CreateZipPackage(fontsZipFilePath, fontFiles);
                            packagedFiles.AddRange(packagedFontFiles.Select(f => $"content/{f}"));
                        }
                    }
                    
                    var musicPath = Path.Combine(contentPath, "music");
                    if (Directory.Exists(musicPath))
                    {
                        var musicFiles = new List<(string sourcePath, string entryName)>();
                        var musicFilePaths = Directory.GetFiles(musicPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var musicFile in musicFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(musicPath, musicFile);
                            musicFiles.Add((musicFile, relativePath));
                        }
                        
                        if (musicFiles.Count > 0)
                        {
                            var musicZipFileName = $"version-{version}-content-music.zip";
                            var musicZipFilePath = Path.Combine(wwwrootPath, musicZipFileName);
                            var packagedMusicFiles = await CreateZipPackage(musicZipFilePath, musicFiles);
                            packagedFiles.AddRange(packagedMusicFiles.Select(f => $"content/{f}"));
                        }
                    }
                    
                    var particlesPath = Path.Combine(contentPath, "particles");
                    if (Directory.Exists(particlesPath))
                    {
                        var particleFiles = new List<(string sourcePath, string entryName)>();
                        var particleFilePaths = Directory.GetFiles(particlesPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var particleFile in particleFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(particlesPath, particleFile);
                            particleFiles.Add((particleFile, relativePath));
                        }
                        
                        if (particleFiles.Count > 0)
                        {
                            var particlesZipFileName = $"version-{version}-content-particles.zip";
                            var particlesZipFilePath = Path.Combine(wwwrootPath, particlesZipFileName);
                            var packagedParticleFiles = await CreateZipPackage(particlesZipFilePath, particleFiles);
                            packagedFiles.AddRange(packagedParticleFiles.Select(f => $"content/{f}"));
                        }
                    }
                    
                    var skyPath = Path.Combine(contentPath, "sky");
                    if (Directory.Exists(skyPath))
                    {
                        var skyFiles = new List<(string sourcePath, string entryName)>();
                        var skyFilePaths = Directory.GetFiles(skyPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var skyFile in skyFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(skyPath, skyFile);
                            skyFiles.Add((skyFile, relativePath));
                        }
                        
                        if (skyFiles.Count > 0)
                        {
                            var skyZipFileName = $"version-{version}-content-sky.zip";
                            var skyZipFilePath = Path.Combine(wwwrootPath, skyZipFileName);
                            var packagedSkyFiles = await CreateZipPackage(skyZipFilePath, skyFiles);
                            packagedFiles.AddRange(packagedSkyFiles.Select(f => $"content/{f}"));
                        }
                    }
                    
                    var soundsPath = Path.Combine(contentPath, "sounds");
                    if (Directory.Exists(soundsPath))
                    {
                        var soundFiles = new List<(string sourcePath, string entryName)>();
                        var soundFilePaths = Directory.GetFiles(soundsPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var soundFile in soundFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(soundsPath, soundFile);
                            soundFiles.Add((soundFile, relativePath));
                        }
                        
                        if (soundFiles.Count > 0)
                        {
                            var soundsZipFileName = $"version-{version}-content-sounds.zip";
                            var soundsZipFilePath = Path.Combine(wwwrootPath, soundsZipFileName);
                            var packagedSoundFiles = await CreateZipPackage(soundsZipFilePath, soundFiles);
                            packagedFiles.AddRange(packagedSoundFiles.Select(f => $"content/{f}"));
                        }
                    }
                
                    var texturesPath = Path.Combine(contentPath, "textures");
                    if (Directory.Exists(texturesPath))
                    {
                        var textureFiles = new List<(string sourcePath, string entryName)>();
                        var textureFilePaths = Directory.GetFiles(texturesPath, "*", SearchOption.AllDirectories);
                        
                        foreach (var textureFile in textureFilePaths)
                        {
                            var relativePath = Path.GetRelativePath(texturesPath, textureFile);
                            textureFiles.Add((textureFile, relativePath));
                        }
                        
                        if (textureFiles.Count > 0)
                        {
                            var texturesZipFileName = $"version-{version}-content-textures.zip";
                            var texturesZipFilePath = Path.Combine(wwwrootPath, texturesZipFileName);
                            var packagedTextureFiles = await CreateZipPackage(texturesZipFilePath, textureFiles);
                            packagedFiles.AddRange(packagedTextureFiles.Select(f => $"content/{f}"));
                        }
                    }
                    
                    var textures2Path = Path.Combine(contentPath, "textures");
                    var wrenchPngPath = Path.Combine(textures2Path, "wrench.png");
                    if (File.Exists(wrenchPngPath))
                    {
                        var texture2Files = new List<(string sourcePath, string entryName)>
                        {
                            (wrenchPngPath, "wrench.png")
                        };
                        
                        var textures2ZipFileName = $"version-{version}-content-textures2.zip";
                        var textures2ZipFilePath = Path.Combine(wwwrootPath, textures2ZipFileName);
                        var packagedTexture2Files = await CreateZipPackage(textures2ZipFilePath, texture2Files);
                        packagedFiles.AddRange(packagedTexture2Files.Select(f => $"content/{f}"));
                    }
                }
                
                var redistFiles = new List<(string sourcePath, string entryName)>();
                var vc90CrtPath = Path.Combine(folderPath, "Microsoft.VC90.CRT");
                if (Directory.Exists(vc90CrtPath))
                {
                    var vc90CrtFiles = Directory.GetFiles(vc90CrtPath, "*", SearchOption.AllDirectories);
                    foreach (var file in vc90CrtFiles)
                    {
                        var relativePath = Path.Combine("Microsoft.VC90.CRT", Path.GetRelativePath(vc90CrtPath, file));
                        redistFiles.Add((file, relativePath));
                    }
                }
                
                var vc90MfcPath = Path.Combine(folderPath, "Microsoft.VC90.MFC");
                if (Directory.Exists(vc90MfcPath))
                {
                    var vc90MfcFiles = Directory.GetFiles(vc90MfcPath, "*", SearchOption.AllDirectories);
                    foreach (var file in vc90MfcFiles)
                    {
                        var relativePath = Path.Combine("Microsoft.VC90.MFC", Path.GetRelativePath(vc90MfcPath, file));
                        redistFiles.Add((file, relativePath));
                    }
                }
                
                var vc90OpenMpPath = Path.Combine(folderPath, "Microsoft.VC90.OPENMP");
                if (Directory.Exists(vc90OpenMpPath))
                {
                    var vc90OpenMpFiles = Directory.GetFiles(vc90OpenMpPath, "*", SearchOption.AllDirectories);
                    foreach (var file in vc90OpenMpFiles)
                    {
                        var relativePath = Path.Combine("Microsoft.VC90.OPENMP", Path.GetRelativePath(vc90OpenMpPath, file));
                        redistFiles.Add((file, relativePath));
                    }
                }
                
                var msvcDlls = Directory.GetFiles(folderPath, "msvc*.dll", SearchOption.TopDirectoryOnly);
                foreach (var dll in msvcDlls)
                {
                    redistFiles.Add((dll, Path.GetFileName(dll)));
                }
                
                var legacyRedistPath = Path.Combine(folderPath, "redist");
                if (Directory.Exists(legacyRedistPath))
                {
                    var legacyFiles = Directory.GetFiles(legacyRedistPath, "*", SearchOption.AllDirectories);
                    foreach (var file in legacyFiles)
                    {
                        var relativePath = Path.Combine("redist", Path.GetRelativePath(legacyRedistPath, file));
                        redistFiles.Add((file, relativePath));
                    }
                }
                
                if (redistFiles.Count > 0)
                {
                    var redistZipFileName = $"version-{version}-redist.zip";
                    var redistZipFilePath = Path.Combine(wwwrootPath, redistZipFileName);
                    var packagedRedistFiles = await CreateZipPackage(redistZipFilePath, redistFiles);
                    packagedFiles.AddRange(packagedRedistFiles.Select(f => $"redist/{f}"));
                }
                
                await GenerateRbxManifestAsync(wwwrootPath, version, packagedFiles, folderPath);
                await GenerateRobloxVersionAsync(wwwrootPath, version, bootstrapperVersion);
                await UpdateClientVersionInDatabaseAsync("WindowsPlayer", version);
                var bootstrapperInfo = (!string.IsNullOrEmpty(bootstrapperPath) && File.Exists(bootstrapperPath)) ? $", file version: {bootstrapperVersion}" : "";
                await WriteToDeploymentHistoryAsync($"New WindowsPlayer version-{version} at {DateTime.Now:dd/MM/yyyy h:mm:ss tt}{bootstrapperInfo}");
                
                return new UploadResult
                {
                    Success = true,
                    UploadId = version,
                    Message = $"Player client packaged successfully: v{version} ({packagedFiles.Count} files)" + 
                             (!string.IsNullOrEmpty(bootstrapperPath) && File.Exists(bootstrapperPath) ? " + bootstrapper" : ""),
                    UploadedFiles = packagedFiles
                };
            }
            catch (Exception ex)
            {
                return new UploadResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Uploads and packages Studio client files (not implemented)
        /// </summary>
        public async Task<UploadResult> UploadStudioClientAsync(string folderPath, string bootstrapperVersion = null)
        {
            return new UploadResult
            {
                Success = false,
                Error = "Studio client upload not implemented yet"
            };
        }
        
        /// <summary>
        /// Uploads and packages RCC client files (not implemented)
        /// </summary>
        public async Task<UploadResult> UploadRccClientAsync(string folderPath, string bootstrapperVersion = null)
        {
            return new UploadResult
            {
                Success = false,
                Error = "RCC client upload not implemented yet"
            };
        }
        
        private async Task<List<string>> CreateZipPackage(string zipFilePath, List<(string sourcePath, string entryName)> files)
        {
            var packagedFiles = new List<string>();
            
            using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (var (sourcePath, entryName) in files)
                {
                    if (File.Exists(sourcePath))
                    {
                        await AddFileToZip(archive, sourcePath, entryName);
                        packagedFiles.Add(entryName);
                    }
                }
            }
            
            return packagedFiles;
        }
        
        private string CalculateMD5Hash(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            var fullHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return fullHash.Substring(0, Math.Min(16, fullHash.Length));
        }
        
        private string CalculateFullMD5Hash(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            var fullHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return fullHash;
        }
        
        private async Task AddFileToZip(System.IO.Compression.ZipArchive archive, string filePath, string entryName)
        {
            var entry = archive.CreateEntry(entryName);
            using var entryStream = entry.Open();
            using var fileStream = File.OpenRead(filePath);
            await fileStream.CopyToAsync(entryStream);
        }
        
        private string GetSetupServiceLocation()
        {
            try
            {
                string setupServiceLocation = null;
                if (_uiDispatcher?.CheckAccess() == true)
                {
                    setupServiceLocation = Settings.Default.SetupServiceLocation;
                }
                else
                {
                    _uiDispatcher?.Invoke(() =>
                    {
                        setupServiceLocation = Settings.Default.SetupServiceLocation;
                    });
                }
                
                if (!string.IsNullOrEmpty(setupServiceLocation) && Directory.Exists(setupServiceLocation))
                {
                    return setupServiceLocation;
                }
            }
            catch
            {
                _logger.LogWarning("Could not read SetupServiceLocation from settings, using default");
            }
            
            var defaultLocation = "C:\\SetupService";
            Directory.CreateDirectory(defaultLocation);
            return defaultLocation;
        }
        
        /// <summary>
        /// Maps Control Panel client types to database client types
        /// </summary>
        private string MapClientType(string controlPanelClientType)
        {
            return controlPanelClientType.ToLowerInvariant() switch
            {
                "player" => "WindowsPlayer",
                "studio" => "Studio", 
                "rcc" => "RCC",
                _ => null
            };
        }
        
        /// <summary>
        /// Updates client version in database setup table
        /// </summary>
        private async Task UpdateClientVersionInDatabaseAsync(string clientType, string version)
        {
            try
            {
                await _dbConnection.OpenAsync();
                
                using var command = _dbConnection.CreateCommand();
                command.CommandText = "SELECT update_client_version(@clientType, @newVersion)";
                command.Parameters.Add(new NpgsqlParameter("@clientType", clientType));
                command.Parameters.Add(new NpgsqlParameter("@newVersion", version));
                
                var result = await command.ExecuteScalarAsync();
                var success = Convert.ToBoolean(result);
                
                if (success)
                {
                }
                else
                {
                    _logger.LogWarning($"Failed to update {clientType} version in database");
                }
            }
            catch (Exception ex)
            {
                LogError("Error updating {clientType} version in database", ex);
            }
            finally
            {
                await _dbConnection.CloseAsync();
            }
        }
        
        public void Dispose()
        {
            _dbConnection?.Dispose();
        }
        
        /// <summary>
        /// Generates rbxManifest.txt with MD5 hashes for all deployed files
        /// </summary>
        private async Task GenerateRbxManifestAsync(string wwwrootPath, string version, List<string> packagedFiles, string sourceFolder)
        {
            try
            {
                var manifestPath = Path.Combine(wwwrootPath, $"version-{version}-rbxManifest.txt");
                var manifestLines = new List<string>();
                var allFiles = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);
                
                foreach (var filePath in allFiles)
                {
                    try
                    {
                        var relativePath = Path.GetRelativePath(sourceFolder, filePath).Replace('\\', '/');
                        var hash = CalculateFullMD5Hash(filePath);
                        manifestLines.Add($"{relativePath}\n{hash}");
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"Failed to process file {filePath}: {ex.Message}");
                    }
                }
                
                await File.WriteAllLinesAsync(manifestPath, manifestLines);
                packagedFiles.Add("rbxManifest.txt");
            }
            catch (Exception ex)
            {
                LogError($"Failed to generate rbxManifest.txt: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets the bootstrapper version from settings or returns default
        /// </summary>
        private async Task<string> GetBootstrapperVersion()
        {
            try
            {
                var settings = await GetBootstrapperSettingsAsync();
                
                var playerVersion = await GetClientVersionAsync("WindowsPlayer");
                return string.IsNullOrEmpty(playerVersion) ? "1, 3, 6, 172" : playerVersion;
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to get bootstrapper version, using default: {ex.Message}");
                return "1, 3, 6, 172";
            }
        }
        
        /// <summary>
        /// Generates RobloxVersion.txt with bootstrapper version
        /// </summary>
        private async Task GenerateRobloxVersionAsync(string wwwrootPath, string version, string bootstrapperVersion = null)
        {
            try
            {
                var versionFilePath = Path.Combine(wwwrootPath, $"version-{version}-RobloxVersion.txt");
                var finalBootstrapperVersion = bootstrapperVersion ?? string.Empty;
                await File.WriteAllTextAsync(versionFilePath, finalBootstrapperVersion);
            }
            catch (Exception ex)
            {
                LogError($"Failed to generate RobloxVersion.txt: {ex.Message}", ex);
            }
        }
    }
    
    /// <summary>
    /// Logger that writes to Control Panel ConsoleWindow instead of console
    /// </summary>
    public class ConsoleWindowLogger : ILogger<SetupService>
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;
        
        public ConsoleWindowLogger(System.Windows.Threading.Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
        
        public IDisposable BeginScope<TState>(TState state) => null;
        
        public bool IsEnabled(LogLevel logLevel) => true;
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            
            if (_dispatcher?.CheckAccess() == true)
            {
                LogToConsoleWindow(logLevel, message);
            }
            else
            {
                try
                {
                    _dispatcher?.BeginInvoke((Action)(() =>
                    {
                        LogToConsoleWindow(logLevel, message);
                    }));
                }
                catch
                {
                    FallbackToConsole(logLevel, message);
                }
            }
        }
        
        private void LogToConsoleWindow(LogLevel logLevel, string message)
        {
            try
            {
                switch (logLevel)
                {
                    case LogLevel.Information:
                    case LogLevel.Debug:
                        ConsoleWindow.Instance?.WriteLine(message);
                        break;
                    case LogLevel.Warning:
                        ConsoleWindow.Instance?.WriteWarning(message);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        ConsoleWindow.Instance?.WriteError(message);
                        break;
                }
            }
            catch
            {
                FallbackToConsole(logLevel, message);
            }
        }
        
        private void FallbackToConsole(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                case LogLevel.Debug:
                    System.Console.WriteLine($"INFO: {message}");
                    break;
                case LogLevel.Warning:
                    System.Console.WriteLine($"WARNING: {message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    System.Console.WriteLine($"ERROR: {message}");
                    break;
            }
        }
    }
    
    public class BootstrapperSettings
    {
        public bool ShowInstallSuccessPrompt { get; set; } = false;
        public string InfluxUrl { get; set; } = "";
        public string InfluxDatabase { get; set; } = "Default";
        public string InfluxUser { get; set; } = "rob";
        public string InfluxPassword { get; set; } = "playfaster";
        public int InfluxInstallHundredthsPercentage { get; set; } = 0;
    }
    
    public class RevertResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Error { get; set; } = "";
        public string RevertedHash { get; set; } = "";
        public List<string> RevertedFiles { get; set; } = new List<string>();
    }
    
    public class UploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Error { get; set; } = "";
        public string UploadId { get; set; } = "";
        public List<string> UploadedFiles { get; set; } = new List<string>();
    }
}
