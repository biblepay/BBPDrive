using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBPDrive
{
    public static class Common
    {
        public static string NormalizeFilePath(string sPath)
        {
            bool IsWindows = true;
            if (IsWindows)
            {
                sPath = sPath.Replace("/", "\\");
                sPath = sPath.Replace("\\\\", "\\");
                return sPath;
            }
            else
            {
                sPath = sPath.Replace("\\", "/");
                sPath = sPath.Replace("//", "/");
                return sPath;
            }
        }
        public static bool IsWindows()
        {
            bool fUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            return !fUnix;
        }


        public static string ChopLastOctetFromURL(string sData)
        {
            string sDelimiter = "/";
            char cDelim = sDelimiter[0];
            string[] vData = sData.Split(cDelim);
            string sOut = "";
            for (int i = 0; i < vData.Length - 1; i++)
            {
                sOut += vData[i] + sDelimiter;
            }
            return sOut;
        }

        public static string ChopLastOctetFromPath(string sData)
        {
            string sDelimiter = IsWindows() ? "\\" : "/";
            string[] vData = sData.Split(sDelimiter[0]);
            string sOut = "";
            for (int i = 0; i < vData.Length - 1; i++)
            {
                sOut += vData[i] + sDelimiter;
            }
            return sOut;
        }

        public static double GetDouble(object o)
        {
            try
            {
                if (o == null) return 0;
                if (o.ToString() == "") return 0;
                double d = Convert.ToDouble(o.ToString());
                return d;
            }
            catch (Exception)
            {
                // Letters?
                return 0;
            }
        }

        public static int UnixTimestamp()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        public static int UnixTimestampSmall()
        {
            int nUT = UnixTimestamp();
            string sData = nUT.ToString().Substring(0, nUT.ToString().Length - 1);
            return (int)GetDouble(sData);
        }

        public static string NormalizeURL(string sURL)
        {
            sURL = sURL.Replace("https://", "{https}");
            sURL = sURL.Replace("///", "/");
            sURL = sURL.Replace("//", "/");
            sURL = sURL.Replace("{https}", "https://");
            sURL = sURL.Replace("\\", "/");

            if (sURL.Substring(0, 1) == "/")
            {
                sURL = sURL.Substring(1, sURL.Length - 1);
            }
            return sURL;
        }



    }
}
