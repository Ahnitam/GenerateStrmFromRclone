using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GenerateStrmFromRclone.Manager
{
    public class Rclone
    {

        public static List<Dictionary<string, dynamic?>> GetRcloneDrives(string rclone, string rcloneConfig)
        {
            try
            {
                Process rcloneProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = rclone,
                        Arguments = String.Format("listremotes --config \"{0}\"", new string[1] { rcloneConfig }),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    }
                };
                rcloneProcess.Start();
                string? standard_output;
                List<Dictionary<string, dynamic?>> drives = new List<Dictionary<string, dynamic?>>();
                while ((standard_output = rcloneProcess.StandardOutput.ReadLine()) != null){
                    drives.Add(new Dictionary<string, dynamic?> { 
                        { "drive", standard_output }
                    });
                }
                rcloneProcess.WaitForExit();
                if (rcloneProcess.ExitCode != 0)
                {
                    rcloneProcess.Dispose();
                    throw new Exception("Error");
                }
                rcloneProcess.Dispose();
                return drives;
            }
            catch (System.Exception)
            {
                return new List<Dictionary<string, dynamic?>>();
            }
        }

        public static List<Dictionary<string, dynamic?>> GetRcloneDrives(string rcloneRcPort)
        {
            try
            {
                List<Dictionary<string, dynamic?>> drives = new List<Dictionary<string, dynamic?>>();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://localhost:{rcloneRcPort}");
                    var response = client.PostAsync("/config/listremotes", null).Result;
                    var content = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception();
                    }

                    JsonElement remotes = content.RootElement.GetProperty("remotes");
                    for (int i = 0; i < remotes.GetArrayLength(); i++)
                    {
                        drives.Add(new Dictionary<string, dynamic?> { 
                            { "drive", remotes[i].GetString() + ":" }
                        });
                    }
                }
                return drives;
            }
            catch (System.Exception)
            {
                return new List<Dictionary<string, dynamic?>>();
            }
        }
        public static bool CheckConfiguration(string? rclonePATH, string? rcloneConfigPATH, string? rcloneRemoteDrive, string? rcloneDrivePATH)
        {
            try
            {
                if (rclonePATH == null || rcloneConfigPATH == null || rcloneRemoteDrive == null)
                {
                    throw new Exception();
                }
                Process rcloneProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = rclonePATH,
                        Arguments = String.Format("lsd --config \"{0}\" \"{1}{2}\"", new string[3] { rcloneConfigPATH, rcloneRemoteDrive, rcloneDrivePATH ?? "" }),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    }
                };
                rcloneProcess.Start();
                rcloneProcess.WaitForExit();
                if (rcloneProcess.ExitCode != 0)
                {
                    rcloneProcess.Dispose();
                    throw new Exception();
                }
                rcloneProcess.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CheckConfiguration(string? rcloneRcPort, string? rcloneRemoteDrive, string? rcloneDrivePATH)
        {
            try
            {
                if (rcloneRcPort == null || rcloneRemoteDrive == null)
                {
                    throw new Exception();
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://localhost:{rcloneRcPort}");
                    var body = new JsonObject {
                        { "fs", rcloneRemoteDrive },
                        { "remote", rcloneDrivePATH ?? "" },
                    };
                    string mediaType = "application/json";

                    var bodyContent = new StringContent(body.ToJsonString(), Encoding.UTF8, mediaType);
                    bodyContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

                    var response = client.PostAsync("/operations/list", bodyContent).Result;

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}