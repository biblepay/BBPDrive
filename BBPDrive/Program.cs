using System;
using System.Linq;
using DokanNet;

namespace BBPDrive
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            try
            {
               
                bool unsafeReadWrite = true; // Using unsafe because Safe goes in endless loop when writing from TextPad
                var mirror = unsafeReadWrite  ? new UnsafeMirror(BBP.GetMountPoint())  : new Mirror(BBP.GetMountPoint());
               
                string sAPIKey = BBPIO.BBP.Config("apikey");
                if (sAPIKey == "")
                {
                    // prompt for it
                    Console.WriteLine("Enter API Key >");
                    string data = Console.ReadLine();
                    if (data.Length < 16)
                    {
                        Console.WriteLine("Invalid key");
                        System.Environment.Exit(1);
                    }
                    else
                    {
                        BBPIO.BBP.WriteConfig("apikey", data);
                    }
                }

                

                // Examples:
                // MountDrive = N:\\
                // MountPoint = C:\\CODE\\PT\\

                Dokan.Init();

                System.Threading.Thread tInbound = new System.Threading.Thread(BBP.InboundThread);
                tInbound.Start();
                System.Threading.Thread tOutbound = new System.Threading.Thread(BBP.OutboundThread);
                tOutbound.Start();

                using (DokanInstance dokanInstance = mirror.CreateFileSystem(BBP.MountDrive, DokanOptions.DebugMode | DokanOptions.EnableNotificationAPI))
                {
                    var notify = new Notify();
                    notify.Start(BBP.GetMountPoint(), BBP.MountDrive, dokanInstance);
                    dokanInstance.WaitForFileSystemClosed(uint.MaxValue);
                }



                Dokan.Shutdown();

                Console.WriteLine("Shutting Down Gracefully...");
            }
            catch (DokanException ex)
            {
                Console.WriteLine("Shutting down with an error: " + ex.Message);
            }
        }
    }
}