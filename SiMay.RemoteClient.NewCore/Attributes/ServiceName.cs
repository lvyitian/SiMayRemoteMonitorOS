using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore.Attributes
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
