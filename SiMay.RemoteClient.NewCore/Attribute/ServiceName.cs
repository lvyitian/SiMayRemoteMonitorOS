using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Service.Core
{
    public class ServiceNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ServiceNameAttribute(string name)
        {
            Name = name;
        }
    }
}
