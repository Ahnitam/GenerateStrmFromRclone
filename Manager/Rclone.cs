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