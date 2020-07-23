using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Core
{
    public class ApplicationKeyAttribute : Attribute
    {
        public string Key { get; set; }
        public ApplicationKeyAttribute(string key) => Key = key;
    }
}
