namespace GenerateStrmFromRclone.Utils
{
    public class Validate
    {
        public static string drivePath(string path)
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

        public static string url(string url)
        {
            return url.EndsWith("/") ? url.TrimEnd('/') : url;
        }

        public static string ip(string Ip)
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
    }
}