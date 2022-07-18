using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Web;
using MediaBrowser.Model.Serialization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net;
using System.Text.Json.Nodes;
using GenerateStrmFromRclone.Model;
using MediaBrowser.Controller;

namespace GenerateStrmFromRclone.ScheduledTasks
{
    public class UpdateStrmFiles : IScheduledTask
    {
        private readonly ILogger<UpdateStrmFiles> _logger;
        private readonly ITaskManager _taskManager;
        private readonly ILocalizationManager _localization;
        private readonly IXmlSerializer _xmlSerializer;
        private readonly IServerApplicationPaths _serverApplicationPaths;

        public UpdateStrmFiles(IServerApplicationPaths serverApplicationPaths, IXmlSerializer xmlSerializer, ITaskManager taskManager, ILogger<UpdateStrmFiles> logger, ILocalizationManager localization)
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
            string rcloneDrivePATH = validateDrivePath(config.rcloneDrivePATH);
            using (var client = new HttpClient())
            {
                try
                {
                    progress.Report(5);
                    client.BaseAddress = new Uri($"http://localhost:{config.rcloneRcPort}");
                    var body = new JsonObject {
                        { "fs", config.rcloneRemoteDrive },
                        { "remote", rcloneDrivePATH },
                        { "opt", new JsonObject {
                                { "recurse", true },
                                { "filesOnly", true },
                                { "showHash", true },
                                { "showEncrypted", false },
                                { "showOrigIDs", false },
                                { "noMimeType", true },
                                { "noModTime", true } ,
                                { "hashTypes", new JsonArray { "MD5" } },
                            }
                        }
                    };
                    string mediaType = "application/json";

                    var bodyContent = new StringContent(body.ToJsonString(), Encoding.UTF8, mediaType);
                    bodyContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                    var response = client.PostAsync("/operations/list", bodyContent).Result;
                    var content = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception();
                    }
                    progress.Report(10);
                    JsonElement newFiles = content.RootElement.GetProperty("list");
                    List<Midia> newFilesList = new List<Midia>();
                    List<Midia> oldFiles = new List<Midia>();
                    int updates = 0;
                    try
                    {
                        oldFiles = (List<Midia>)_xmlSerializer.DeserializeFromFile(typeof(List<Midia>), Path.Join(_serverApplicationPaths.PluginConfigurationsPath, "GenerateStrmFromRclone_StrmFiles.xml"));
                    }
                    catch (Exception)
                    {
                        _logger.LogInformation("GenerateStrmFromRclone_StrmFiles.xml not found");
                    }
                    for (int i = 0; i < newFiles.GetArrayLength() && !cancellationToken.IsCancellationRequested; i++)
                    {
                        progress.Report(10 + (((i + 1) * 80) / newFiles.GetArrayLength()));
                        JsonElement newFile = newFiles[i];
                        string? newFileName = Path.GetFileNameWithoutExtension(newFile.GetProperty("Name").GetString());
                        string? newFileExtension = Path.GetExtension(newFile.GetProperty("Name").GetString());

                        if (newFileExtension?.ToUpper() == ".MKV" || newFileExtension?.ToUpper() == ".MP4" || newFileExtension?.ToUpper() == ".TS" || newFileExtension?.ToUpper() == ".WEBM" || newFileExtension?.ToUpper() == ".AVI" || newFileExtension?.ToUpper() == ".M4V")
                        {
                            string newFilePath = Path.GetDirectoryName(newFile.GetProperty("Path").GetString()) ?? string.Empty;
                            string? newFileMd5 = newFile.GetProperty("Hashes").GetProperty("md5").GetString();

                            if (newFilePath.StartsWith(rcloneDrivePATH))
                            {
                                newFilePath = rcloneDrivePATH.Length == newFilePath.Length ? string.Empty : newFilePath.Substring(rcloneDrivePATH.Length + (rcloneDrivePATH == string.Empty ? 0 : 1));
                            }

                            List<string> tempPaths = new List<string> { config.rcloneMediaPATH! };
                            tempPaths.AddRange(newFilePath.Split("/"));
                            tempPaths.Add(newFileName + ".strm");

                            string newFilePathLocal = Path.Join(tempPaths.ToArray());

                            Midia? oldFile = oldFiles.FirstOrDefault(x => x.PATH == newFilePathLocal);

                            if (oldFile == null || oldFile.MD5 != newFileMd5)
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(newFilePathLocal)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(newFilePathLocal)!);
                                }
                                string url = $"http://{ValidateIp(config.rcloneServeIP!)}:{config.rcloneServePort}/{HttpUtility.UrlPathEncode(Path.Join(newFilePath, newFileName + newFileExtension))}";
                                using (StreamWriter sw = File.CreateText(newFilePathLocal))
                                {
                                    sw.WriteLine(url);
                                }
                                updates++;
                            }
                            else
                            {
                                oldFiles.Remove(oldFile);
                            }
                            newFilesList.Add(new Midia
                            {
                                PATH = newFilePathLocal,
                                MD5 = newFileMd5
                            });
                        }
                    }
                    progress.Report(90);
                    for (int i = 0; i < oldFiles.Count && !cancellationToken.IsCancellationRequested; i++)
                    {
                        progress.Report(90 + (((i + 1) * 9) / oldFiles.Count));
                        var item = oldFiles[i];
                        try
                        {
                            File.Delete(item.PATH!);
                            if (Directory.GetFiles(Path.GetDirectoryName(item.PATH!)!).Length == 0)
                            {
                                Directory.Delete(Path.GetDirectoryName(item.PATH!)!);
                            }
                        }
                        catch (System.Exception)
                        {
                            _logger.LogInformation("Error deleting file: " + item.PATH);
                        }
                    }
                    progress.Report(99);
                    if (updates != 0 && !cancellationToken.IsCancellationRequested)
                    {
                        _xmlSerializer.SerializeToFile(newFilesList, Path.Join(_serverApplicationPaths.PluginConfigurationsPath, "GenerateStrmFromRclone_StrmFiles.xml"));
                    }
                    progress.Report(100);
                    _logger.LogInformation($"{updates} Atualizações feitas");

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
                    _logger.LogError("Verifique as configurações e reiniciei o servidor", e);
                    return Task.FromException(e);
                }
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

        public string Name => "Atualizar Arquivos Strm";
        public string Key => "UpdateStrmFiles";
        public string Description => "Verifica se há atualizações no drive e atualiza/cria os arquivos strm";
        public string Category => _localization.GetLocalizedString("TasksLibraryCategory");
    }
}