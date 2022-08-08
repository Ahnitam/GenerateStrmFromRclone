using System.Diagnostics;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Plugins;
using GenerateStrmFromRclone.Configuration;
using Microsoft.Extensions.Logging;
using GenerateStrmFromRclone.Manager;

namespace GenerateStrmFromRclone.ServerEntryPoints
{
    public class RcloneServer : IServerEntryPoint
    {
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly ILogger<RcloneServer> _logger;
        private readonly INetworkManager _networkManager;
        private bool disposedValue = false;
        private Process? rcloneHTTPProcess;

        public RcloneServer(INetworkManager networkManager, IServerConfigurationManager serverConfigurationManager, ILogger<RcloneServer> logger)
        {
            this.rcloneHTTPProcess = null;
            this._logger = logger;
            this._serverConfigurationManager = serverConfigurationManager;
            this._networkManager = networkManager;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (this.rcloneHTTPProcess != null)
                {
                    this.rcloneHTTPProcess.Kill(true);
                    this.rcloneHTTPProcess.WaitForExit();
                    this.rcloneHTTPProcess.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private string ValidateIp(string Ip)
        {
            if (Ip.Contains(":") && !Ip.Contains("[") && !Ip.Contains("]"))
            {
                return "[" + Ip + "]";
            }else
            {
                return Ip;
            }
        }

        public Task RunAsync()
        {
            PluginConfiguration config = Plugin.Instance!.Configuration;
            if (
                config.rcloneOption == "automatic" && Rclone.CheckConfiguration(config.rclonePATH, config.rcloneConfigPATH, config.rcloneRemoteDrive, config.rcloneDrivePATH) &&
                config.rcloneServeIP != null
            )
            {
                try
                {
                    this.rcloneHTTPProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = config.rclonePATH,
                            Arguments = String.Format("serve http --rc --rc-addr localhost:{0} --rc-no-auth --config \"{1}\" --addr \"{2}:{3}\" --read-only \"{4}{5}\"", new string[6] { config.rcloneRcPort.ToString(), config.rcloneConfigPATH!, ValidateIp(config.rcloneServeIP), config.rcloneServePort.ToString(), config.rcloneRemoteDrive!, config.rcloneDrivePATH }),
                            WindowStyle = ProcessWindowStyle.Hidden,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        }
                    };
                    _logger.LogInformation("Iniciando rclone HTTP server");
                    this.rcloneHTTPProcess.Start();
                    try
                    {
                        if (this.rcloneHTTPProcess.WaitForExit(10000)){
                            throw new Exception("rclone HTTP server error");
                        }
                        _logger.LogInformation("Rclone server iniciado com sucesso");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    if (this.rcloneHTTPProcess != null)
                    {
                        this.rcloneHTTPProcess.Kill(true);
                        this.rcloneHTTPProcess.WaitForExit();
                        this.rcloneHTTPProcess.Dispose();
                    }
                    _logger.LogError("Rclone HTTP server: " + e.Message);
                    this.rcloneHTTPProcess = null;
                }
            }
            else if (config.rcloneOption == "automatic")
            {
                _logger.LogError("Configure para poder iniciar, após configurar reinicie o servidor");
            }
            return Task.CompletedTask;
        }
    }
}