using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SiMay.Basic
{
    public class SecurityHelper
    {
        public static string MD5(string text)
        {
            using (System.Security.Cryptography.MD5 mi = System.Security.Cryptography.MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes(text);
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
