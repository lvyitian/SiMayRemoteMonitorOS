using SiMay.Platform.Windows;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class RegSeekerMatch : EntitySerializerBase
    {
        public string Key { get; set; }

        public RegValueData[] Data { get; set; }

        public bool HasSubKeys { get; set; }

        public override string ToString()
        {
            return $"({Key}:{Data})";
        }
    }
}
