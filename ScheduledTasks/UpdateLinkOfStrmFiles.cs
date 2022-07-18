using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Serialization;
using GenerateStrmFromRclone.Model;
using MediaBrowser.Controller;
using System.Text.RegularExpressions;

namespace GenerateStrmFromRclone.ScheduledTasks
{
    public class UpdateLinksOfStrmFiles : IScheduledTask
    {
        private readonly ILogger<UpdateLinksOfStrmFiles> _logger;
        private readonly ITaskManager _taskManager;
        private readonly ILocalizationManager _localization;
        private readonly IXmlSerializer _xmlSerializer;
        private readonly IServerApplicationPaths _serverApplicationPaths;

        public UpdateLinksOfStrmFiles(IServerApplicationPaths serverApplicationPaths, IXmlSerializer xmlSerializer, ITaskManager taskManager, ILogger<UpdateLinksOfStrmFiles> logger, ILocalizationManager localization)
        {
            _logger = logger;
            _taskManager = taskManager;
            _localization = localization;
            _xmlSerializer = xmlSerializer;
            _serverApplicationPaths = serverApplicationPaths;
        }

        protected string validateDrivePath(string path)
        {
            string temp = path.Trim();
            bool initWithBarra = temp.StartsWith("/");
            bool endWithBarra = temp.EndsWith("/");

            if (initWithBarra && endWithBarra)
            {
                temp = path.Trim(new char[] { ' ', '/' });
            }
            else if (initWithBarra)
            {
                temp = path.TrimStart(new char[] { ' ', '/' });
            }
            else if (endWithBarra)
            {
                temp = path.TrimEnd(new char[] { ' ', '/' });
            }
            else
            {
                temp = path;
            }
            return temp;
        }

        private string ValidateIp(string Ip)
        {
            if (Ip.Contains(":") && !Ip.Contains("[") && !Ip.Contains("]"))
            {
                return "[" + Ip + "]";
            }
            else
            {
                return Ip;
            }
        }

        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var config = Plugin.Instance!.Configuration;
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string[] Files = Directory.GetFiles(config.rcloneMediaPATH!, "*.strm", SearchOption.AllDirectories);
                for (int i = 0; i < Files.Count(); i++)
                {
                    progress.Report(((i + 1) * 100) / Files.Count());
                    string file = Files[i];
                    try
                    {
                        string url = File.ReadAllText(file); 
                        url = Regex.Replace(url, @"http:\/\/.*?:\d{4,}\/", $"http://{ValidateIp(config.rcloneServeIP!)}:{config.rcloneServePort}/", RegexOptions.None);
                        using (StreamWriter sw = File.CreateText(file))
                        {
                            sw.WriteLine(url);
                        }
                    }
                    catch (Exception)
                    {
                        _logger.LogError($"Error updateing link of strm file {file}");
                    }
                }

                progress.Report(100);
                foreach (IScheduledTaskWorker item in _taskManager.ScheduledTasks)
                {
                    if (item.ScheduledTask.Key == "RefreshLibrary")
                    {
                        _taskManager.Execute(item, new TaskOptions());
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GenerateStrmFromRclone_StrmFiles.xml not found", e);
                throw;
            }
            

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield break;
        }

        public string Name => "Atualizar Links de Arquivos Strm";
        public string Key => "UpdateLinkStrmFiles";
        public string Description => "Atualizar links dos arquivos strm com base na configuração atual do servidor rclone";
        public string Category => _localization.GetLocalizedString("TasksLibraryCategory");
    }
}