using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileHelper
    {
        /// <summary>
        /// List of illegal path characters.
        /// </summary>
        private static readonly char[] IllegalPathChars = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();

        /// <summary>
        /// Indicates if the given path contains illegal characters.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>Returns <value>true</value> if the path contains illegal characters, otherwise <value>false</value>.</returns>
        public static bool HasIllegalCharacters(string path)
        {
            return !path.Any(c => IllegalPathChars.Contains(c));
        }

        public static bool VerifyLongPath(string path)
        {
            return path.Length >= 260 || path.Substring(0, path.LastIndexOf("\\")).Length >= 248;
        }
        public static string LengthToFileSize(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            string filesize = String.Format("{0:0.##} {1}", len, sizes[order]);
            return filesize;
        }
    }
}
