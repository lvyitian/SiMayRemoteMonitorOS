using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileExceptionPack : EntitySerializerBase
    {
        public DateTime OccurredTime { get; set; }
        public string TipMessage { get; set; }
        public string StackTrace { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
