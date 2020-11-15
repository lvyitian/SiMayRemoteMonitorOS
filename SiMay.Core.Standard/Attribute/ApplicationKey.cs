using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Core
{
    public class ApplicationServiceKeyAttribute : Attribute
    {
        public string Key { get; set; }
        public ApplicationServiceKeyAttribute(string key) => Key = key;
    }
}
