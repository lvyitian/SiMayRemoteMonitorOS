using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ServiceKeyAttribute : Attribute
    {
        public string Key { get; set; }
        public ServiceKeyAttribute(string key) 
            => Key = key;
    }
}
