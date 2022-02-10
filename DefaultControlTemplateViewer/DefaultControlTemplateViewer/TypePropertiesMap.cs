using System;
using System.Collections.Generic;
using System.Reflection;

namespace DefaultControlTemplateViewer
{
    public class TypePropertiesMap
    {
        public List<PropertyInfo> TypeProperties { get; set; } = new List<PropertyInfo>();
        public Type? Type { get; set; }
    }
}
