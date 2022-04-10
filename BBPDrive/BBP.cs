using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BBPDrive.Common;

namespace BBPDrive
{
    public static class BBP
    {
        public static string CDN = "https://globalcdn.biblepay.org:8443";
        public static string MountDrive = "N:\\";
        public static Dictionary<string, long> dOutboundQueue = new Dictionary<string, long>();

        public static string GetMountPoint()
        {
            string sMP =  BBPIO.BBP.GetAppDataFolder() + "\\cache\\";
            return sMP;
        }

        public static void AddOutboundQueue(string sFilePath, bool fDelete)
        {
            if (!dOutboundQueue.ContainsKey(sFilePath))
            {
                dOutboundQueue.Add(sFilePath, Common.UnixTimestamp());
            }
            else
            {
                dOutboundQueue[sFilePath] = UnixTimestamp();
            }
            if (fDelete)
            {
                dOutboundQueue[sFilePath] = 9;
            }
        }


        public async static void OutboundThread()
        {
            System.Threading.Thread.Sleep(3000);

            // This thread handles files in the queue that need to go OUT to the sancs...
            string sKey = BBPIO.BBP.Config("apikey");

            if (sKey.Length < 16)
            {
                BBPIO.BBP.Log("Error instantiating outbound thread; bad api key");
                Environment.Exit(1);
            }
            while (true)
            {
                try
                {
                    foreach (KeyValuePair<string, long> entry in dOutboundQueue)
                    {
                        if (entry.Value == 9)
                        {
                            // Delete the file
                            dOutboundQueue[entry.Key] = 0;
                            string sSourceFile = BBP.GetMountPoint() + entry.Key;
                            if (true)
                            {
                                string sDestURL = NormalizeURL(entry.Key);
                                bool fSuccess = await BBPIO.BBP.UploadFileToSanc(CDN, sKey, sSourceFile, sDestURL, true);
                                if (!fSuccess)
                                {
                                    BBPIO.BBP.Log("Unable to delete file " + sDestURL);
                                }
                                bool f1 = false;
                            }

                        }
                        else
                        {
                            long nElapsed = Common.UnixTimestamp() - entry.Value;
                            if (nElapsed > 10 && entry.Value != 9)
                            {
                                // After ten seconds of idle (this ensures file is not being written to), we shoot out the file to the Sanc, and we clear the timestamp.
                                dOutboundQueue[entry.Key] = 0;
                                string sSourceFile = BBP.GetMountPoint() + entry.Key;
                                if (System.IO.File.Exists(sSourceFile))
                                {
                                    string sDestURL = NormalizeURL(entry.Key);
                                    bool fSuccess = await BBPIO.BBP.UploadFileToSanc(CDN, sKey, sSourceFile, sDestURL, false);
                                    if (!fSuccess)
                                    {
                                        BBPIO.BBP.Log("Unable to write file " + sDestURL + " to egress...");                                    
                                    }
                                    bool f1 = false;
                                }

                            }
                        }
                        
                    }
                }catch(Exception ex)
                {
                    // Collection was modified, enumeration may not continue
                }

                for (int i = 0; i < 5; i++)
                {
                    // In case we need to break out...
                    System.Threading.Thread.Sleep(1000);
                }

            }

        }

        public async static void InboundThread()
        {
            // This thread handles files that are in the Sanctuary that need to come INTO the local machine
            System.Threading.Thread.Sleep(3000);

            string sKey = BBPIO.BBP.Config("apikey");
            if (sKey == "")
            {
                BBPIO.BBP.Log("Error instantiating inbound thread::Invalid API Key");
                System.Environment.Exit(1);
            }
            string uid = await BBPIO.BBP.ValidateAPIKey(BBP.CDN, sKey);
            if (uid == "")
            {
                BBPIO.BBP.Log("Unable to create inbound thread::API Key invalid");
                System.Environment.Exit(1);
            }
            string sPrefix = "video/" + uid + "/";

            Console.WriteLine("Successfully started inbound service.");
            string sURLEx = CDN + "/" + sPrefix;
            Console.WriteLine("Your files are available at " + sURLEx);

            while (true)
            {
            
                List<string> l = await BBPIO.BBP.GetDirectoryContents(CDN, sKey);
                for (int i = 0; i < l.Count; i++)
                {
                    string sItem = l[i];
                    string sLocalItem = sItem.Replace(sPrefix, "");
                    string sRemoteItem = NormalizeFilePath(sLocalItem);
                    string sFullPath = NormalizeFilePath(BBP.GetMountPoint() + sRemoteItem);
                    string sURL = NormalizeURL(CDN + "/" + sItem);
                    if (!System.IO.File.Exists(sFullPath))
                    {
                        string sDir = ChopLastOctetFromPath(sFullPath);
                        if (!System.IO.Directory.Exists(sDir))
                        {
                            System.IO.Directory.CreateDirectory(sDir);
                        }
                        try
                        {
                            BBPIO.BBPWebClient b = new BBPIO.BBPWebClient();
                            b.DownloadFile(sURL, sFullPath);
                        }
                        catch(Exception ex)
                        {
                            // Possibly a 404?
                            BBPIO.BBP.Log("Unable to retrieve file " + sRemoteItem);
                        }
                    }
                }
                // Set up a flag to break out of the loop if we need to sync...
                for (int i = 0; i < 60; i++)
                {
                    // In case we need to break out...
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

    }
}
