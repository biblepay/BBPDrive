using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace BBPIO
{

        public static class BBP
        {

            public async static Task<bool> UploadFileToSanc(string sCDN, string sAPIKey, string sFilePath, string sURL, bool fDeleteFile)
            {
                try
                {
                    string sEP = sCDN + "/api/web/bbpingress";
                    using (var httpClient = new System.Net.Http.HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), sEP))
                        {
                            httpClient.Timeout = new System.TimeSpan(0, 60, 00);
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Key", sAPIKey);
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("url", sURL);
                            var multipartContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
                          
                            if (fDeleteFile)
                            {
                                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("delete", "1");
                            }
                            else
                            {
                                System.Net.Http.HttpContent bytesContent = new ByteArrayContent(System.IO.File.ReadAllBytes(sFilePath));
                                multipartContent.Add(bytesContent, "file", System.IO.Path.GetFileName(sFilePath));
                            }
                            request.Content = multipartContent;
                            //ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                            var oInitialResponse = await httpClient.PostAsync(sEP, multipartContent);
                            string sJsonResponse = await oInitialResponse.Content.ReadAsStringAsync();
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    BBP.Log("UploadFileToSanc::" + ex.Message);
                    return false;
                }
            }


        public async static Task<List<string>> GetDirectoryContents(string sCDN, string sAPIKey)
        {
            List<string> l = new List<string>();
            try
            {
                string sEP = sCDN + "/BMS/GetDirectoryContents?key=" + sAPIKey;
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), sEP))
                    {
                        httpClient.Timeout = new System.TimeSpan(0, 60, 00);
                        var oInitialResponse = await httpClient.PostAsync(sEP, null);
                        string sJsonResponse = await oInitialResponse.Content.ReadAsStringAsync();
                        l = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(sJsonResponse);
                        return l;
                    }
                }
            }
            catch (Exception)
            {
                return l;
            }
        }


        public static async Task<string> ValidateAPIKey(string sCDN, string sAPIKey)
        {
            // Returns either a uid or empty if invalid
            try
            {
                string sEP = sCDN + "/BMS/ValidateKey?key=" + sAPIKey;
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), sEP))
                    {
                        httpClient.Timeout = new System.TimeSpan(0, 60, 00);
                        var oInitialResponse = await httpClient.PostAsync(sEP, null);
                        string sJsonResponse = await oInitialResponse.Content.ReadAsStringAsync();
                        dynamic u = Newtonsoft.Json.JsonConvert.DeserializeObject(sJsonResponse);
                        string uid = u.userid;
                        return uid;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }


        public static string GetAppDataFolder()
        {
            bool fUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            string sPath = fUnix ? "/" : "c:\\";
            string sRoot = fUnix ? Environment.GetEnvironmentVariable("HOME") : Environment.ExpandEnvironmentVariables("%APPDATA%");
            string sHome = System.IO.Path.Combine(sRoot, "bbpdrive");
            if (!System.IO.Directory.Exists(sHome))
            {
                System.IO.Directory.CreateDirectory(sHome);
            }
            string sCache = System.IO.Path.Combine(sHome, "cache");
            if (!System.IO.Directory.Exists(sCache))
            {
                System.IO.Directory.CreateDirectory(sCache);
            }
            return sHome;
        }


        public static string Config(string _Key)
        {
            string sPath =  GetAppDataFolder() + "\\bbpdrive.conf";
            string sData = System.IO.File.ReadAllText(sPath);
            char delimn = "\n"[0];
            char delime = "="[0];

            string[] vData = sData.Split(delimn);
            for (int i = 0; i < vData.Length; i++)
            {
                string sEntry = vData[i];
                sEntry = sEntry.Replace("\r", "");
                string[] vRow = sEntry.Split(delime);
                if (vRow.Length >= 2)
                {
                    string sKey = vRow[0];
                    string sValue = vRow[1];
                    if (sKey.ToLower() == _Key.ToLower())
                        return sValue;
                }

            }
            return string.Empty;
        }

        public static void WriteConfig(string _Key, string _Value)
        {
            string sPath = GetAppDataFolder() + "\\bbpdrive.conf";
            if (!System.IO.File.Exists(sPath))
            {
                System.IO.File.WriteAllText(sPath, "");
            }
            string sData = System.IO.File.ReadAllText(sPath);
            char delimn = "\n"[0];
            char delime = "="[0];
            string sNewPath = GetAppDataFolder() + "\\bbpdrive.temp";
            StreamWriter fOut = new StreamWriter(sNewPath);
            string[] vData = sData.Split(delimn);
            bool fFound = false;
            for (int i = 0; i < vData.Length; i++)
            {
                string sEntry = vData[i];
                sEntry = sEntry.Replace("\r", "");
                string[] vRow = sEntry.Split(delime);
                if (vRow.Length >= 2)
                {
                    string sKey = vRow[0];
                    string sValue = vRow[1];
                    if (sKey.ToLower() == _Key.ToLower())
                    {
                        sValue = _Value;
                        fFound = true;
                    }
                    string sNewEntry = sKey + "=" + sValue;
                    fOut.WriteLine(sNewEntry);
                }

            }
            if (!fFound)
            {
                string sNewEntry = _Key + "=" + _Value;
                fOut.WriteLine(sNewEntry);
            }
            fOut.Close();
            System.IO.File.Delete(sPath);
            System.IO.File.Copy(sNewPath, sPath);
            System.IO.File.Delete(sNewPath);
        }

        private static string GetExecutingAssemblyFolder()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            string sTheDir = System.IO.Path.GetDirectoryName(path);
            return sTheDir;
        }
        public static void Log(string sData)
        {
            try
            {
                string sPath = GetAppDataFolder() + "\\log.log";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(sPath, true);
                string Timestamp = DateTime.Now.ToString();
                sw.WriteLine(Timestamp + ": " + sData);
                sw.Close();
            }
            catch (Exception ex)
            {
                string sMsg = ex.Message;
            }
        }
    }

    public class BBPWebClient : System.Net.WebClient
    {
        private int DEFAULT_TIMEOUT = 30000;

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            r.Timeout = DEFAULT_TIMEOUT;
            var request = r as HttpWebRequest;
            return r;
        }

        public BBPWebClient()
        {

        }

        public void SetTimeout(int nTimeOut)
        {
            DEFAULT_TIMEOUT = nTimeOut;
        }
    }
}

